using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Ranger.Common;
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.RabbitMQ.BusSubscriber
{
    public class BusSubscriber : BusSubscriberBase, IBusSubscriber
    {
        private bool _disposedValue;
        private readonly ILogger<BusSubscriber> _logger;

        public BusSubscriber(ILifetimeScope lifetimeScope, IConnection connection, IBusPublisher busPublisher, RabbitMQOptions options, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
        : base(lifetimeScope, connection, busPublisher, options, applicationLifetime, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BusSubscriber>();
        }

        public IBusSubscriber SubscribeCommandWithCallback<TCommand>(Func<TCommand, ICorrelationContext, Task> onReceived, Func<TCommand, RangerException, IRejectedEvent> onError = null)
            where TCommand : ICommand
        {
            base.Subscribe<TCommand>(onReceived, onError);
            _logger.LogInformation($"Subscribed to command queue: '{base.QueueName(typeof(TCommand))}' on exchange: '{base.ExchangeName(typeof(TCommand))}'");
            return this;
        }

        public IBusSubscriber SubscribeCommandWithHandler<TCommand>(Func<TCommand, RangerException, IRejectedEvent> onError = null)
            where TCommand : ICommand
        {
            base.Subscribe<TCommand>(onError: onError);
            _logger.LogInformation($"Subscribed to command queue: '{base.QueueName(typeof(TCommand))}' on exchange: '{base.ExchangeName(typeof(TCommand))}'");
            return this;
        }

        public IBusSubscriber SubscribeEventWithCallback<TEvent>(Func<TEvent, ICorrelationContext, Task> onReceived, Func<TEvent, RangerException, IRejectedEvent> onError = null)
            where TEvent : IEvent
        {
            base.Subscribe<TEvent>(onReceived, onError);
            _logger.LogInformation($"Subscribed to event queue: '{base.QueueName(typeof(TEvent))}' on exchange: '{base.ExchangeName(typeof(TEvent))}'");
            return this;
        }

        public IBusSubscriber SubscribeEventWithHandler<TEvent>(Func<TEvent, RangerException, IRejectedEvent> onError = null)
            where TEvent : IEvent
        {
            base.Subscribe<TEvent>(onError: onError);
            _logger.LogInformation($"Subscribed to event queue: '{base.QueueName(typeof(TEvent))}' on exchange: '{base.ExchangeName(typeof(TEvent))}'");
            return this;
        }

        public async Task UnsubscribeCommand<TCommand>() where TCommand : ICommand
        {
            await base.Unsubscribe<TCommand>();
            _logger.LogInformation($"Unsubscribed from comand queue: '{base.QueueName(typeof(TCommand))}' on exchange: '{base.ExchangeName(typeof(TCommand))}'");
        }

        public async Task UnsubscribeEvent<TEvent>() where TEvent : IEvent
        {
            await base.Unsubscribe<TEvent>();
            _logger.LogInformation($"Unsubscribed from event queue: '{base.QueueName(typeof(TEvent))}' on exchange: '{base.ExchangeName(typeof(TEvent))}'");
        }

        protected new virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    base.Dispose();
                }
                _disposedValue = true;
            }
        }

        public new void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}