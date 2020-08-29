using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Ranger.Common;

namespace Ranger.RabbitMQ
{
    public class BusSubscriber : IBusSubscriber
    {
        private const int consumerCancelDelayMs = 100;

        private readonly ILogger<BusSubscriber> _logger;
        private readonly IComponentContext _componentContext;
        private IConnection _connection;
        private readonly IBusPublisher _publisher;
        private readonly RabbitMQOptions _options;
        private readonly TimeSpan retryInterval;
        private IModel _channel;

        private ConcurrentBag<int> executingConsumerCount = new ConcurrentBag<int>();
        private readonly ConsumerTagManager consumerTagManager = new ConsumerTagManager();

        public BusSubscriber(IComponentContext componentContext, IConnection rabbitMqConnection, IBusPublisher busPublisher, RabbitMQOptions options, ILogger<BusSubscriber> logger)
        {
            _componentContext = componentContext;
            _connection = rabbitMqConnection;
            _publisher = busPublisher;
            _options = options;
            _logger = logger;
            _channel = _connection.CreateModel();
            retryInterval = new TimeSpan(0, 0, options.RetryInterval > 0 ? options.RetryInterval : 2);
        }

        public IBusSubscriber SubscribeCommand<TCommand>(Func<TCommand, ICorrelationContext, Task> onReceived, Func<TCommand, RangerException, IRejectedEvent> onError = null)
            where TCommand : ICommand
        {
            var (queueName, exchangeName) = subscribeMessage<TCommand>(onReceived, onError);
            _logger.LogInformation($"Subscribed to command queue: '{queueName}' on exchange: '{exchangeName}'");
            return this;
        }

        public IBusSubscriber SubscribeCommand<TCommand>(Func<TCommand, RangerException, IRejectedEvent> onError = null)
            where TCommand : ICommand
        {
            var (queueName, exchangeName) = subscribeMessage<TCommand>(onError: onError);
            _logger.LogInformation($"Subscribed to command queue: '{queueName}' on exchange: '{exchangeName}'");
            return this;
        }

        public IBusSubscriber SubscribeEvent<TEvent>(Func<TEvent, ICorrelationContext, Task> onReceived, Func<TEvent, RangerException, IRejectedEvent> onError = null)
            where TEvent : IEvent
        {
            var (queueName, exchangeName) = subscribeMessage<TEvent>(onReceived, onError);
            _logger.LogInformation($"Subscribed to event queue: '{queueName}' on exchange: '{exchangeName}'");
            return this;
        }

        public IBusSubscriber SubscribeEvent<TEvent>(Func<TEvent, RangerException, IRejectedEvent> onError = null)
            where TEvent : IEvent
        {
            var (queueName, exchangeName) = subscribeMessage<TEvent>(onError: onError);
            _logger.LogInformation($"Subscribed to event queue: '{queueName}' on exchange: '{exchangeName}'");
            return this;
        }

        public void UnSubscribeCommand<TCommand>() where TCommand : ICommand
        {
            var (queueName, exchangeName) = UnSubscribeMessage<TCommand>();
            _logger.LogInformation($"Unsubscribed from comand queue: '{queueName}' on exchange: '{exchangeName}'");
        }

        public void UnSubscribeEvent<TEvent>() where TEvent : IEvent
        {
            var (queueName, exchangeName) = UnSubscribeMessage<TEvent>();
            _logger.LogInformation($"Unsubscribed from event queue: '{queueName}' on exchange: '{exchangeName}'");
        }

        private (string QueueName, string ExchangeName) UnSubscribeMessage<TMessage>()
            where TMessage : IMessage
        {
            string consumerTag;
            consumerTagManager.TryGetValue(typeof(TMessage), out consumerTag);
            if (String.IsNullOrWhiteSpace(consumerTag))
            {
                throw new ArgumentException("No consumer tag was found for the requested type");
            }
            _channel.BasicCancel(consumerTag);
            var exchangeName = NamingConventions.ExchangeNamingConvention(typeof(TMessage), _options.Namespace);
            var queueName = NamingConventions.QueueNamingConvention(typeof(TMessage), _options.Namespace);
            return (queueName, exchangeName);
        }

        private (string QueueName, string ExchangeName) subscribeMessage<TMessage>(Func<TMessage, ICorrelationContext, Task> onReceived = null, Func<TMessage, RangerException, IRejectedEvent> onError = null) where TMessage : IMessage
        {
            consumerTagManager.Add(typeof(TMessage), "");
            var exchangeName = NamingConventions.ExchangeNamingConvention(typeof(TMessage), _options.Namespace);
            var queueName = NamingConventions.QueueNamingConvention(typeof(TMessage), _options.Namespace);
            var routingKey = NamingConventions.RoutingKeyConvention(typeof(TMessage), _options.Namespace);
            _channel.Bind<TMessage>(exchangeName, queueName, routingKey, _options);

            var eventingConsumer = RegisterConsumerEvents(queueName, onReceived, onError);

            _channel.BasicConsume(
                queueName,
                false,
                eventingConsumer
            );
            return (queueName, exchangeName);
        }

        private AsyncEventingBasicConsumer RegisterConsumerEvents<TMessage>(string queueName, Func<TMessage, ICorrelationContext, Task> onReceived = null, Func<TMessage, RangerException, IRejectedEvent> onError = null) where TMessage : IMessage
        {
            var eventingConsumer = new AsyncEventingBasicConsumer(_channel);
            eventingConsumer.ConsumerCancelled += async (ch, ea) =>
            {
                await Task.Run(() =>
                    {
                        var closureReason = ((EventingBasicConsumer)ch).Model.CloseReason;
                        _logger.LogInformation("Consumer cancelled, closure reason: {ClosureReason}", closureReason);
                    });
            };

            eventingConsumer.Registered += (ch, ea) => Task.Run(() =>
            {
                _logger.LogDebug("Stashing eventing consumer tags: {ConsumerTags}", String.Join(',', ea.ConsumerTags));
                ea.ConsumerTags.ToList().ForEach(ct => consumerTagManager.Update(typeof(TMessage), ct));
            });

            eventingConsumer.Received += async (ch, ea) =>
            {
                executingConsumerCount.Add(1);
                try
                {
                    _logger.LogDebug($"Received message from queue: '{queueName}'");
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
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                        _logger.LogDebug($"Message from queue '{queueName}' nack'd");
                        return;
                    }
                    var messageState = await TryHandleAsync(message, context, onReceived, onError);
                    if (messageState is MessageState.Failed)
                    {
                        _logger.LogError($"Sending message: '{message.GetType().Name}' with correlation id: '{context.CorrelationContextId}' to the error queue");
                        _publisher.Error<TMessage>(message, ea);
                    }
                    _channel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogDebug($"Message from queue '{queueName}' ack'd");
                }
                finally
                {
                    executingConsumerCount.TryTake(out _);
                }
            };

            return eventingConsumer;
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
                    _logger.LogInformation("Handling message: {MessageName}' with correlation id: '{CorrelationId}'. {RetryCount}", messageName, context.CorrelationContextId, retryMessage);

                    if (onReceived is null)
                    {
                        _logger.LogDebug("Resolving message handler from ServiceProvider");
                        var messageHandler = _componentContext.Resolve<IMessageHandler<TMessage>>();
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

                    messageState = MessageState.Succeeded;
                    _logger.LogInformation("Handled message: {MessageName}' with correlation id: '{CorrelationId}'. {RetryCount}", messageName, context.CorrelationContextId, retryMessage);
                }
                catch (Exception ex)
                {
                    if (ex is RangerException rangerException && !(onError is null))
                    {
                        var rejectedEvent = onError(message, rangerException);
                        _publisher.Publish(rejectedEvent, context);
                        _logger.LogWarning($"Published a rejected event: '{rejectedEvent.GetType().Name}' for the message: '{messageName}' with correlation id: '{context.CorrelationContextId}'");
                        messageState = MessageState.Rejected;
                        break;
                    }

                    _logger.LogError(ex, "Unable to handle message: {MessageName}' with correlation id: '{CorrelationId}'. {RetryCount}/{RetryMaxAttempts}", messageName, context.CorrelationContextId, currentRetry, _options.Retries);

                    currentRetry++;
                    context.IncrementRetries();
                    await Task.Delay(retryInterval);
                }
            }
            return messageState;
        }

        public async ValueTask DisposeAsync()
        {
            _logger.LogDebug("DisposeAsync() called");
            await DisposeAsyncCore();
            _logger.LogInformation("DisposeAsync() complete");
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            foreach (var consumerTag in consumerTagManager.ConsumerTags)
            {
                _channel.BasicCancel(consumerTag);
            }

            while (!executingConsumerCount.IsEmpty)
            {
                _logger.LogInformation("Waiting for consumers to finish. Checking again {Delay}", consumerCancelDelayMs);
                await Task.Delay(consumerCancelDelayMs);
            }

            _channel.Close();
            _channel.Dispose();
        }
    }
}