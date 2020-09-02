using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Ranger.Common;
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.RabbitMQ.BusSubscriber
{
    public class BusSubscriber : BusSubscriberBase, IBusSubscriber
    {
        private readonly ILogger<BusSubscriber> _logger;

        public BusSubscriber(ILifetimeScope lifetimeScope, IConnection connection, IBusPublisher busPublisher, RabbitMQOptions options, ILogger<BusSubscriber> logger)
        : base(lifetimeScope, connection, busPublisher, options, logger)
        {
            _logger = logger;
        }

        public IBusSubscriber SubscribeCommandWithCallback<TCommand>(Func<TCommand, ICorrelationContext, Task> onReceived, Func<TCommand, RangerException, IRejectedEvent> onError = null)
            where TCommand : ICommand
        {
            subscribeMessage<TCommand>(onReceived, onError);
            _logger.LogInformation($"Subscribed to command queue: '{base.QueueName<TCommand>()}' on exchange: '{base.ExchangeName<TCommand>()}'");
            return this;
        }

        public IBusSubscriber SubscribeCommandWithHandler<TCommand>(Func<TCommand, RangerException, IRejectedEvent> onError = null)
            where TCommand : ICommand
        {
            subscribeMessage<TCommand>(onError: onError);
            _logger.LogInformation($"Subscribed to command queue: '{base.QueueName<TCommand>()}' on exchange: '{base.ExchangeName<TCommand>()}'");
            return this;
        }

        public IBusSubscriber SubscribeEventWithCallback<TEvent>(Func<TEvent, ICorrelationContext, Task> onReceived, Func<TEvent, RangerException, IRejectedEvent> onError = null)
            where TEvent : IEvent
        {
            subscribeMessage<TEvent>(onReceived, onError);
            _logger.LogInformation($"Subscribed to event queue: '{base.QueueName<TEvent>()}' on exchange: '{base.ExchangeName<TEvent>()}'");
            return this;
        }

        public IBusSubscriber SubscribeEventWithHandler<TEvent>(Func<TEvent, RangerException, IRejectedEvent> onError = null)
            where TEvent : IEvent
        {
            subscribeMessage<TEvent>(onError: onError);
            _logger.LogInformation($"Subscribed to event queue: '{base.QueueName<TEvent>()}' on exchange: '{base.ExchangeName<TEvent>()}'");
            return this;
        }

        private void subscribeMessage<TMessage>(Func<TMessage, ICorrelationContext, Task> onReceived = null, Func<TMessage, RangerException, IRejectedEvent> onError = null) where TMessage : IMessage
        {
            base.ConsumerTagManager.Add(typeof(TMessage), "");
            Channel.Bind<TMessage>(base.ExchangeName<TMessage>(), base.QueueName<TMessage>(), base.RoutingKey<TMessage>(), base.Options);

            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.ConsumerCancelled += base.ConsumerCancelled;
            consumer.Registered += base.ConsumerRegistered<TMessage>;
            consumer.Received += (sender, eventArgs) => base.MessageReceived<TMessage>(sender, eventArgs, onReceived, onError);

            Channel.BasicConsume(
                base.QueueName<TMessage>(),
                false,
                consumer
            );
        }

        public void UnsubscribeCommand<TCommand>() where TCommand : ICommand
        {
            unubscribeMessage<TCommand>();
            _logger.LogInformation($"Unsubscribed from comand queue: '{base.QueueName<TCommand>()}' on exchange: '{base.ExchangeName<TCommand>()}'");
        }

        public void UnsubscribeEvent<TEvent>() where TEvent : IEvent
        {
            unubscribeMessage<TEvent>();
            _logger.LogInformation($"Unsubscribed from event queue: '{base.QueueName<TEvent>()}' on exchange: '{base.ExchangeName<TEvent>()}'");
        }

        private void unubscribeMessage<TMessage>()
            where TMessage : IMessage
        {
            string consumerTag;
            base.ConsumerTagManager.TryGetValue(typeof(TMessage), out consumerTag);
            if (String.IsNullOrWhiteSpace(consumerTag))
            {
                throw new ArgumentException("No consumer tag was found for the requested type");
            }
            Channel.BasicCancel(consumerTag);

        }

        public async ValueTask DisposeAsync()
        {
            _logger.LogDebug("BusSubscriber.DisposeAsync() called");
            await base.DisposeAsyncCore();
            _logger.LogInformation("BusSubscriber.DisposeAsync() complete");
        }
    }
}