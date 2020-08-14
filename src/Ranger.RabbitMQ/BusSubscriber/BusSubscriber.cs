using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
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
        private readonly ILogger<BusSubscriber> logger;
        private readonly IBusPublisher publisher;
        private readonly RabbitMQOptions options;
        private readonly TimeSpan retryInterval;
        private readonly IServiceProvider serviceProvider;
        private readonly IModel channel;

        public BusSubscriber(IApplicationBuilder app)
        {
            serviceProvider = app.ApplicationServices;
            logger = serviceProvider.GetRequiredService<ILogger<BusSubscriber>>();
            publisher = serviceProvider.GetRequiredService<IBusPublisher>();
            options = serviceProvider.GetRequiredService<RabbitMQOptions>();
            retryInterval = new TimeSpan(0, 0, options.RetryInterval > 0 ? options.RetryInterval : 2);
            var connection = serviceProvider.GetRequiredService<IConnection>();
            channel = connection.CreateModel();
        }

        public IBusSubscriber SubscribeCommand<TCommand>(Func<TCommand, RangerException, IRejectedEvent> onError = null) where TCommand : ICommand
        {
            var exchangeName = NamingConventions.ExchangeNamingConvention(typeof(TCommand), options.Namespace);
            var queueName = NamingConventions.QueueNamingConvention(typeof(TCommand), options.Namespace);
            var routingKey = NamingConventions.RoutingKeyConvention(typeof(TCommand), options.Namespace);
            channel.Bind<TCommand>(exchangeName, queueName, routingKey, options);

            var eventingConsumer = RegisterConsumerEvents(queueName, onError);

            channel.BasicConsume(
                queueName,
                false,
                eventingConsumer
            );

            logger.LogInformation($"Subscribed to command queue: '{queueName}' on exchange: '{exchangeName}'");
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

            logger.LogInformation($"Subscribed to event queue: '{queueName}' on exchange: '{exchangeName}'");
            return this;
        }

        private AsyncEventingBasicConsumer RegisterConsumerEvents<TMessage>(string queueName, Func<TMessage, RangerException, IRejectedEvent> onError = null) where TMessage : IMessage
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var eventingConsumer = new AsyncEventingBasicConsumer(channel);
            eventingConsumer.ConsumerCancelled += async (ch, ea) =>
            {
                await Task.Run(() =>
                    {
                        var closureReason = ((EventingBasicConsumer)ch).Model.CloseReason;
                        logger.LogInformation("Consumer cancelled, closure reason: {ClosureReason}", closureReason);
                    });
            };
            eventingConsumer.Received += async (ch, ea) =>
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    logger.LogInformation("Consumer cancellation requested");
                    channel.BasicNack(ea.DeliveryTag, false, false);
                    logger.LogInformation($"Message from queue '{queueName}' nack'd");
                    this.Dispose();
                }
                else
                {
                    logger.LogDebug($"Received message from queue: '{queueName}'");
                    TMessage message = default(TMessage);
                    CorrelationContext context = default(CorrelationContext);
                    try
                    {
                        message = JsonConvert.DeserializeObject<TMessage>(System.Text.Encoding.Default.GetString(ea.Body.Span));
                        context = CorrelationContext.FromBasicDeliverEventArgs(ea);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Failed to deserialize message of type: '{message.GetType()}' with rabbitmq message id: '{ea.BasicProperties.MessageId}'. Sending to dead letter exchange");
                        channel.BasicNack(ea.DeliveryTag, false, false);
                        logger.LogDebug($"Message from queue '{queueName}' nack'd");
                        return;
                    }
                    var messageState = await TryHandleAsync(message, context, onError);
                    if (messageState is MessageState.Failed)
                    {
                        logger.LogError($"Sending message: '{message.GetType().Name}' with correlation id: '{context.CorrelationContextId}' to the error queue");
                        publisher.Error<TMessage>(message, ea);
                    }
                    channel.BasicAck(ea.DeliveryTag, false);
                    logger.LogDebug($"Message from queue '{queueName}' ack'd");
                }
            };
            return eventingConsumer;
        }

        private async Task<MessageState> TryHandleAsync<TMessage>(TMessage message, CorrelationContext context, Func<TMessage, RangerException, IRejectedEvent> onError = null)
        where TMessage : IMessage
        {
            var currentRetry = 0;
            var messageName = message.GetType().Name;
            var messageState = MessageState.Failed;

            //TODO: Add Polly retry policy
            while (currentRetry <= options.Retries && messageState is MessageState.Failed)
            {
                try
                {
                    var retryMessage = currentRetry == 0 ?
                        string.Empty :
                        $"Retry: {currentRetry}'.";
                    logger.LogInformation($"Handling message: '{messageName}' with correlation id: '{context.CorrelationContextId}'. {retryMessage}");

                    var messageHandler = serviceProvider.GetRequiredService<IMessageHandler<TMessage>>();
                    //TODO: This would be better handled if on startup we could determine a misconfiguration
                    if (messageHandler is null)
                    {
                        throw new NullReferenceException($"Unable to locate message handler in service collection for message: '{messageName}'");
                    }
                    await messageHandler.HandleAsync(message, context);

                    messageState = MessageState.Succeeded;
                    logger.LogInformation($"Handled message: '{messageName}' with correlation id: '{context.CorrelationContextId}'. {retryMessage}");
                }
                catch (Exception ex)
                {
                    if (ex is RangerException rangerException && !(onError is null))
                    {
                        var rejectedEvent = onError(message, rangerException);
                        publisher.Publish(rejectedEvent, context);
                        logger.LogWarning($"Published a rejected event: '{rejectedEvent.GetType().Name}' for the message: '{messageName}' with correlation id: '{context.CorrelationContextId}'");
                        messageState = MessageState.Rejected;
                        break;
                    }

                    logger.LogError(ex, $"Unable to handle message: '{messageName}' with correlation id: '{context.CorrelationContextId}', retry {currentRetry}/{options.Retries}");

                    currentRetry++;
                    context.IncrementRetries();
                    await Task.Delay(retryInterval);
                }
            }
            return messageState;
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
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            logger.LogDebug("BusSubscriber.Dispose() called");
            Dispose(true);
        }
    }
}