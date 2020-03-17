using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Ranger.Common;

namespace Ranger.RabbitMQ
{
    public class BusSubscriber : IBusSubscriber
    {
        private const int ConnectionRetryDuration = 10000;
        private const int ConnectionMaxRetrys = 9;
        private readonly ILogger<BusSubscriber> logger;
        private readonly IBusPublisher publisher;
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly RabbitMQOptions options;
        private readonly TimeSpan retryInterval;
        private readonly IServiceProvider serviceProvider;

        public BusSubscriber(IApplicationBuilder app)
        {
            serviceProvider = app.ApplicationServices.GetService<IServiceProvider>();
            logger = serviceProvider.GetService<ILogger<BusSubscriber>>();
            bool connected = false;
            int connectionAttempt = 0;
            while (!connected && connectionAttempt <= ConnectionMaxRetrys)
            {
                try
                {
                    connection = serviceProvider.GetService<IConnectionFactory>().CreateConnection();
                }
                catch (BrokerUnreachableException ex)
                {
                    logger.LogCritical(ex, $"Failed to connect to RabbitMQ broker. Retrying in {ConnectionRetryDuration / 1000} seconds. Connection attempt: {connectionAttempt}.");
                    if (connectionAttempt == ConnectionMaxRetrys)
                    {
                        logger.LogCritical("Abandoning RabbitMQ Connection.");
                        throw;
                    }
                    connectionAttempt++;
                    Thread.Sleep(ConnectionRetryDuration);
                    continue;
                }
                connected = true;
                logger.LogInformation("Subscriber connected.");
            }
            publisher = serviceProvider.GetService<IBusPublisher>();
            options = serviceProvider.GetService<RabbitMQOptions>();
            retryInterval = new TimeSpan(0, 0, options.RetryInterval > 0 ? options.RetryInterval : 2);
            channel = connection.CreateModel();
        }

        public IBusSubscriber SubscribeCommand<TCommand>(Func<TCommand, RangerException, IRejectedEvent> onError = null) where TCommand : ICommand
        {
            var exchangeName = NamingConventions.ExchangeNamingConvention(typeof(TCommand), options.Namespace);
            var queueName = NamingConventions.QueueNamingConvention(typeof(TCommand), options.Namespace);
            var routingKey = NamingConventions.RoutingKeyConvention(typeof(TCommand), options.Namespace);
            channel.Bind<TCommand>(exchangeName, queueName, routingKey, options);

            AsyncEventingBasicConsumer eventingConsumer = RegisterConsumerEvents(queueName, onError);

            channel.BasicConsume(
                queueName,
                false,
                eventingConsumer
            );

            logger.LogInformation($"Subscribed to command queue: '{queueName}' " +
                $"on exchange: '{exchangeName}'.");
            return this;
        }

        public IBusSubscriber SubscribeEvent<TEvent>(Func<TEvent, RangerException, IRejectedEvent> onError = null) where TEvent : IEvent
        {
            var exchangeName = NamingConventions.ExchangeNamingConvention(typeof(TEvent), options.Namespace);
            var queueName = NamingConventions.QueueNamingConvention(typeof(TEvent), options.Namespace);
            var routingKey = NamingConventions.RoutingKeyConvention(typeof(TEvent), options.Namespace);
            channel.Bind<TEvent>(exchangeName, queueName, routingKey, options);

            AsyncEventingBasicConsumer eventingConsumer = RegisterConsumerEvents(queueName, onError);

            channel.BasicConsume(
                queueName,
                false,
                eventingConsumer
            );

            logger.LogInformation($"Subscribed to event queue: '{queueName}' " +
                $"on exchange: '{exchangeName}'.");
            return this;
        }

        private AsyncEventingBasicConsumer RegisterConsumerEvents<TMessage>(string queueName, Func<TMessage, RangerException, IRejectedEvent> onError = null) where TMessage : IMessage
        {
            var eventingConsumer = new AsyncEventingBasicConsumer(channel);
            eventingConsumer.ConsumerCancelled += async (ch, ea) => { await Task.Run(() => logger.LogInformation("Subscriber cancelled.")); };
            eventingConsumer.Registered += async (ch, ea) => { await Task.Run(() => logger.LogInformation($"Subscriber registered.")); };
            eventingConsumer.Unregistered += async (ch, ea) => { await Task.Run(() => logger.LogInformation($"Subscriber unregistered.")); };
            eventingConsumer.Shutdown += async (ch, ea) => { await Task.Run(() => logger.LogInformation($"Subscriber shutdown.")); };
            eventingConsumer.Received += async (ch, ea) =>
            {
                logger.LogDebug($"Received message from queue: '{queueName}'.");
                TMessage message = default(TMessage);
                CorrelationContext context = default(CorrelationContext);
                try
                {
                    message = JsonConvert.DeserializeObject<TMessage>(System.Text.Encoding.Default.GetString(ea.Body), new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore });
                    context = CorrelationContext.FromBasicDeliverEventArgs(ea);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to deserialize message of type: '{message.GetType()}' " +
                        $"with rabbitmq message id: '{ea.BasicProperties.MessageId}'. Sending to dead letter exchange.");
                    channel.BasicNack(ea.DeliveryTag, false, false);
                    logger.LogDebug($"Message from queue '{queueName}' nack'd.");
                    return;
                }
                var success = await TryHandleAsync(message, context, onError);
                if (!success)
                {
                    logger.LogError($"Sending message: '{message.GetType().Name}' " +
                        $"with correlation id: '{context.CorrelationContextId}', " +
                        "to the error queue.");
                    publisher.Error<TMessage>(message, ea);
                }
                channel.BasicAck(ea.DeliveryTag, false);
                logger.LogDebug($"Message from queue '{queueName}' ack'd.");
            };
            return eventingConsumer;
        }

        private async Task<bool> TryHandleAsync<TMessage>(TMessage message, CorrelationContext context, Func<TMessage, RangerException, IRejectedEvent> onError = null)
        where TMessage : IMessage
        {
            var currentRetry = 0;
            var messageName = message.GetType().Name;
            var success = false;

            while (currentRetry <= options.Retries && !success)
            {
                try
                {
                    var retryMessage = currentRetry == 0 ?
                        string.Empty :
                        $"Retry: {currentRetry}'.";
                    var preLogMessage = $"Handling message: '{messageName}' " +
                        $"with correlation id: '{context.CorrelationContextId}'. {retryMessage}";
                    logger.LogInformation(preLogMessage);

                    try
                    {
                        var messageHandler = serviceProvider.GetService<IMessageHandler<TMessage>>();
                        await messageHandler.HandleAsync(message, context);
                    }
                    catch (NullReferenceException)
                    {
                        //TODO: This would be better handled if on startup we could determine a misconfiguration
                        logger.LogError($"Unable to locate message handler in service collection for message: '{messageName}'");
                        throw;
                    }
                    success = true;
                    logger.LogInformation($"Handled message: '{messageName}' " +
                        $"with correlation id: '{context.CorrelationContextId}'. {retryMessage}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                    if (ex is RangerException rangerException && onError != null)
                    {
                        var rejectedEvent = onError(message, rangerException);
                        publisher.Publish(rejectedEvent, context);
                        logger.LogWarning($"Published a rejected event: '{rejectedEvent.GetType().Name}' " +
                            $"for the message: '{messageName}' with correlation id: '{context.CorrelationContextId}'.");
                        break;
                    }

                    logger.LogError(ex, $"Unable to handle message: '{messageName}' " +
                        $"with correlation id: '{context.CorrelationContextId}', " +
                        $"retry {currentRetry}/{options.Retries}...");

                    currentRetry++;
                    context.IncrementRetries();
                    await Task.Delay(retryInterval);

                }
            }
            return success;
        }

        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    channel.Close();
                    channel.Dispose();
                    connection.Close();
                    connection.Dispose();
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            logger.LogDebug("BusSubscriber.Dispose() called.");
            Dispose(true);
        }

    }
}