using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Ranger.Common;
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.RabbitMQ.BusSubscriber
{
    public class BusSubscriberBase
    {

        protected readonly ConsumerTagManager ConsumerTagManager = new ConsumerTagManager();
        protected readonly RabbitMQOptions Options;
        protected IModel Channel;

        private IConnection _connection;
        private const int consumerCancelDelayMs = 100;
        private ConcurrentBag<int> executingConsumerCount = new ConcurrentBag<int>();
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IBusPublisher _busPublisher;
        private readonly ILogger _logger;
        private readonly TimeSpan _retryInterval;

        public BusSubscriberBase(ILifetimeScope lifetimeScope, IConnection connection, IBusPublisher busPublisher, RabbitMQOptions options, ILogger<BusSubscriber> logger)
        {
            _retryInterval = new TimeSpan(0, 0, options.RetryInterval > 0 ? options.RetryInterval : 2);
            _lifetimeScope = lifetimeScope;
            _connection = connection;
            _busPublisher = busPublisher;
            _logger = logger;
            Channel = connection.CreateModel();
            Options = options;

        }

        protected Task ConsumerCancelled(object sender, ConsumerEventArgs ea)
        {
            var closureReason = ((EventingBasicConsumer)sender).Model.CloseReason;
            _logger.LogInformation("Consumer cancelled, closure reason: {ClosureReason}", closureReason);
            return Task.CompletedTask;
        }

        protected Task ConsumerRegistered<TMessage>(object sender, ConsumerEventArgs ea)
            where TMessage : IMessage
        {
            _logger.LogDebug("Stashing eventing consumer tags: {ConsumerTags}", String.Join(',', ea.ConsumerTags));
            ea.ConsumerTags.ToList().ForEach(ct => ConsumerTagManager.Update(typeof(TMessage), ct));
            return Task.CompletedTask;
        }

        protected async Task MessageReceived<TMessage>(object sender, BasicDeliverEventArgs ea, Func<TMessage, ICorrelationContext, Task> onReceived = null, Func<TMessage, RangerException, IRejectedEvent> onError = null)
            where TMessage : IMessage
        {
            executingConsumerCount.Add(1);
            try
            {

                _logger.LogDebug($"Received message from queue: '{QueueName<TMessage>()}'");
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
                    Channel.BasicNack(ea.DeliveryTag, false, false);
                    _logger.LogDebug($"Message from queue '{QueueName<TMessage>()}' nack'd");
                    return;
                }
                var messageState = await TryHandleAsync(message, context, onReceived, onError);
                if (messageState is MessageState.Cancelled)
                {
                    _logger.LogWarning($"Nacking and requeueing message: '{message.GetType().Name}' with correlation id: '{context.CorrelationContextId}'");
                    Channel.BasicNack(ea.DeliveryTag, false, true);
                }
                else if (messageState is MessageState.Failed)
                {
                    _logger.LogError($"Sending message: '{message.GetType().Name}' with correlation id: '{context.CorrelationContextId}' to the error queue");
                    _busPublisher.Error<TMessage>(message, ea);
                }
                else
                {
                    Channel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogDebug($"Message from queue '{QueueName<TMessage>()}' ack'd");
                }
            }
            finally
            {
                executingConsumerCount.TryTake(out _);
            }
        }

        private async Task<MessageState> TryHandleAsync<TMessage>(TMessage message, CorrelationContext context, Func<TMessage, ICorrelationContext, Task> onReceived = null, Func<TMessage, RangerException, IRejectedEvent> onError = null)
            where TMessage : IMessage
        {
            var currentRetry = 0;
            var messageName = message.GetType().Name;
            var messageState = MessageState.Failed;

            //TODO: Add Polly retry policy
            while (currentRetry <= Options.Retries && messageState is MessageState.Failed && !(messageState is MessageState.Cancelled))
            {
                try
                {
                    var retryMessage = currentRetry == 0 ?
                        string.Empty :
                        $"Retry: {currentRetry}'.";
                    _logger.LogInformation("Handling message: {MessageName}' with correlation id: '{CorrelationId}'. {RetryCount}", messageName, context.CorrelationContextId, retryMessage);

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

                            _logger.LogInformation("Handled message: {MessageName}' with correlation id: '{CorrelationId}'. {RetryCount}", messageName, context.CorrelationContextId, retryMessage);
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

                    _logger.LogError(ex, "Unable to handle message: {MessageName}' with correlation id: '{CorrelationId}'. {RetryCount}/{RetryMaxAttempts}", messageName, context.CorrelationContextId, currentRetry, Options.Retries);

                    currentRetry++;
                    context.IncrementRetries();
                    await Task.Delay(_retryInterval);
                }
            }
            return messageState;
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            foreach (var consumerTag in ConsumerTagManager.ConsumerTags)
            {
                Channel.BasicCancel(consumerTag);
            }

            while (!executingConsumerCount.IsEmpty)
            {
                _logger.LogInformation("Waiting for consumers to finish. Checking again {Delay}", consumerCancelDelayMs);
                await Task.Delay(consumerCancelDelayMs);
            }

            Channel.Close();
            Channel.Dispose();
        }

        protected string ExchangeName<TMessage>() => NamingConventions.ExchangeNamingConvention(typeof(TMessage), Options.Namespace);
        protected string QueueName<TMessage>() => NamingConventions.QueueNamingConvention(typeof(TMessage), Options.Namespace);
        protected string RoutingKey<TMessage>() => NamingConventions.RoutingKeyConvention(typeof(TMessage), Options.Namespace);
    }
}