using System;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ranger.RabbitMQ.BusPublisher
{
    public class BusPublisher<TStartup> : BusPublisherBase<TStartup>, IBusPublisher
        where TStartup : class
    {
        private bool _disposedValue;
        private readonly ILogger<BusPublisher<TStartup>> _logger;

        public BusPublisher(IConnection connection, RabbitMQOptions options, ILoggerFactory loggerFactory)
            : base(connection, options, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BusPublisher<TStartup>>();
            ConfigureChannel();
            InitializeTopology();
        }

        protected override void ConfigureChannel()
        {
            Channel.ConfirmSelect();
            Channel.BasicAcks += (sender, ea) => CleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
            Channel.BasicNacks += (sender, ea) => logNacks(ea.DeliveryTag, ea.Multiple);
            _logger.LogInformation("Publisher connected");
        }

        public void Publish<TEvent>(TEvent @event, ICorrelationContext context = null) where TEvent : IEvent
        {
            ChannelPublish<TEvent>(@event, context);
        }

        public void Send<TCommand>(TCommand command, ICorrelationContext context = null) where TCommand : ICommand
        {
            ChannelPublish<TCommand>(command, context);
        }

        public void Error<TMessage>(TMessage message, BasicDeliverEventArgs ea) where TMessage : IMessage
        {
            ErrorPublish<TMessage>(message, ea);
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