using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Ranger.Common;
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.RabbitMQ.BusSubscriber
{
    public class BusSubscriberBase : IDisposable
    {
        private readonly ChannelManager ChannelManager = new ChannelManager();
        private readonly IConnection _connection;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IBusPublisher _busPublisher;
        private readonly ILoggerFactory _loggerFactory;
        private readonly RabbitMQOptions _options;
        private readonly TimeSpan _retryInterval;
        private readonly ILogger _logger;
        private bool _disposedValue;

        public BusSubscriberBase(ILifetimeScope lifetimeScope, IConnection connection, IBusPublisher busPublisher, RabbitMQOptions options, IHostApplicationLifetime hostApplicationLifetime, ILoggerFactory loggerFactory)
        {
            _retryInterval = new TimeSpan(0, 0, options.RetryInterval > 0 ? options.RetryInterval : 2);
            _lifetimeScope = lifetimeScope;
            _connection = connection;
            _busPublisher = busPublisher;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<BusSubscriberBase>();
            _options = options;
        }

        protected IModel Subscribe<TMessage>(Func<TMessage, ICorrelationContext, Task> onReceived = null, Func<TMessage, RangerException, IRejectedEvent> onError = null)
            where TMessage : IMessage
        {
            var channel = _connection.CreateModel();
            channel.BasicQos(0, 50, true);
            channel.Bind<TMessage>(ExchangeName(typeof(TMessage)), QueueName(typeof(TMessage)), RoutingKey(typeof(TMessage)), _options);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Shutdown += Shutdown;
            consumer.ConsumerCancelled += ConsumerCancelled;
            consumer.Received += (sender, eventArgs) => MessageReceived<TMessage>(sender, eventArgs, onReceived, onError);

            var managedChannel = new ManagedChannel(channel, consumer, _loggerFactory.CreateLogger<ManagedChannel>());
            ChannelManager.Add(typeof(TMessage), managedChannel);

            channel.BasicConsume(
                QueueName(typeof(TMessage)),
                false,
                consumer
            );
            return channel;
        }

        private Task Shutdown(object sender, ShutdownEventArgs ea)
        {
            var initiator = ea.Initiator;
            _logger.LogInformation("Consumer shutdown, initiator: {Initiator}", initiator);
            return Task.CompletedTask;
        }

        protected Task ConsumerCancelled(object sender, ConsumerEventArgs ea)
        {
            var closureReason = ((EventingBasicConsumer)sender).Model.CloseReason;
            _logger.LogInformation("Consumer cancelled, closure reason: {ClosureReason}", closureReason);
            return Task.CompletedTask;
        }

        protected async Task MessageReceived<TMessage>(object sender, BasicDeliverEventArgs ea, Func<TMessage, ICorrelationContext, Task> onReceived = null, Func<TMessage, RangerException, IRejectedEvent> onError = null)
            where TMessage : IMessage
        {
            _logger.LogDebug($"Received message from queue: '{QueueName(typeof(TMessage))}'");
            ManagedChannel managedChannel = default;
            ChannelManager.TryGetValue(typeof(TMessage), out managedChannel);
            try
            {
                managedChannel.Lock();
                TMessage message = default(TMessage);
                CorrelationContext context = default(CorrelationContext);
                try
                {
                    message = JsonConvert.DeserializeObject<TMessage>(System.Text.Encoding.Default.GetString(ea.Body.Span));
                    context = CorrelationContext.FromBasicDeliverEventArgs(ea);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to deserialize message of type: '{message.GetType()}' with rabbitmq message id: '{ea.BasicProperties.MessageId}'. Sending to dead letter exchange");
                    managedChannel.Nack(ea.DeliveryTag, false);
                    _logger.LogDebug($"Message from queue '{QueueName(typeof(TMessage))}' nack'd");
                    return;
                }
                var messageState = await TryHandleAsync(message, context, onReceived, onError);

                if (messageState is MessageState.Cancelled)
                {
                    _logger.LogWarning($"Nacking and requeueing message: '{message.GetType().Name}' with correlation id: '{context.CorrelationContextId}'");
                    managedChannel.Nack(ea.DeliveryTag, true);
                    return;
                }
                if (messageState is MessageState.Failed)
                {
                    _logger.LogError($"Sending message: '{message.GetType().Name}' with correlation id: '{context.CorrelationContextId}' to the error queue");
                    _busPublisher.Error<TMessage>(message, ea);
                }
                managedChannel.Ack(ea.DeliveryTag);
                _logger.LogDebug($"Message from queue '{QueueName(typeof(TMessage))}' ack'd");
            }
            finally
            {
                managedChannel.Release();
            }
        }

        private async Task<MessageState> TryHandleAsync<TMessage>(TMessage message, CorrelationContext context, Func<TMessage, ICorrelationContext, Task> onReceived = null, Func<TMessage, RangerException, IRejectedEvent> onError = null)
            where TMessage : IMessage
        {
            var currentRetry = 0;
            var messageName = message.GetType().Name;
            var messageState = MessageState.Failed;

            //TODO: Add Polly retry policy
            while (currentRetry <= _options.Retries && messageState is MessageState.Failed)
            {
                try
                {
                    var retryMessage = currentRetry == 0 ?
                        string.Empty :
                        $"Retry: {currentRetry}'.";
                    _logger.LogDebug("Handling message: {MessageName}' with correlation id: '{CorrelationId}'. {RetryCount}", messageName, context.CorrelationContextId, retryMessage);

                    try
                    {
                        using (var scope = _lifetimeScope.BeginLifetimeScope())
                        {
                            if (onReceived is null)
                            {
                                _logger.LogDebug("Resolving message handler from ServiceProvider");
                                var messageHandler = _lifetimeScope.Resolve<IMessageHandler<TMessage>>();
                                if (messageHandler is null)
                                {
                                    throw new NullReferenceException($"Unable to locate message handler in service collection for message: '{messageName}'");
                                }
                                await messageHandler.HandleAsync(message, context);
                            }
                            else
                            {
                                _logger.LogDebug("Using delegate message handler");
                                await onReceived(message, context);
                            }

                            _logger.LogDebug("Handled message: {MessageName}' with correlation id: '{CorrelationId}'. {RetryCount}", messageName, context.CorrelationContextId, retryMessage);
                            messageState = MessageState.Succeeded;
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        _logger.LogInformation("The container has been disposed. Cancelling handler for message: {MessageName}' with correlation id: '{CorrelationId}'. {RetryCount}", messageName, context.CorrelationContextId, retryMessage);
                        messageState = MessageState.Cancelled;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is RangerException rangerException && !(onError is null))
                    {
                        var rejectedEvent = onError(message, rangerException);
                        _busPublisher.Publish(rejectedEvent, context);
                        _logger.LogWarning($"Published a rejected event: '{rejectedEvent.GetType().Name}' for the message: '{messageName}' with correlation id: '{context.CorrelationContextId}'");
                        messageState = MessageState.Rejected;
                        break;
                    }

                    _logger.LogError(ex, "Unable to handle message: {MessageName}' with correlation id: '{CorrelationId}'. {RetryCount}/{RetryMaxAttempts}", messageName, context.CorrelationContextId, currentRetry, _options.Retries);

                    currentRetry++;
                    context.IncrementRetries();
                    await Task.Delay(_retryInterval);
                }
            }
            return messageState;
        }

        protected async Task Unsubscribe<TMessage>() where TMessage : IMessage
        {
            ManagedChannel channel = default;
            if (ChannelManager.TryGetValue(typeof(TMessage), out channel))
            {
                await channel.DisposeAsync();
                ChannelManager.Remove(typeof(TMessage));
            }
            else
            {
                throw new ArgumentException("No ManagedChannel was found for the requested type");
            }
        }

        protected string ExchangeName(Type messageType) => NamingConventions.ExchangeNamingConvention(messageType, _options.Namespace);
        protected string QueueName(Type messageType) => NamingConventions.QueueNamingConvention(messageType, _options.Namespace);
        protected string RoutingKey(Type messageType) => NamingConventions.RoutingKeyConvention(messageType, _options.Namespace);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _disposedValue = true;
                if (disposing)
                {
                    var types = ChannelManager.Keys.ToArray();
                    var consumerCancellationTasks = new List<Task>();
                    foreach (var type in types)
                    {
                        ManagedChannel managedChannel = default;
                        ChannelManager.TryGetValue(type, out managedChannel);
                        _logger.LogInformation("Adding cancellation task for message: {MessageName}", type.Name);
                        consumerCancellationTasks.Add(managedChannel.DisposeAsync());
                    }
                    Task.WaitAll(consumerCancellationTasks.ToArray());
                    foreach (var type in types)
                    {
                        ChannelManager.Remove(type);
                    }
                    _connection.Close();
                    _connection.Dispose();
                }
            }
        }



        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}