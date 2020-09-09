using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Autofac;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ranger.RabbitMQ.BusPublisher
{
    public class BusPublisherWithOutbox<TStartup, TDbContext> : BusPublisherWithOutboxBase<TStartup, TDbContext>, IBusPublisher
        where TStartup : class
        where TDbContext : DbContext, IOutboxStore
    {
        private bool _disposedValue;

        public BusPublisherWithOutbox(IConnection connection,
                                      RabbitMQOptions options,
                                      IDataProtectionProvider dataProtectionProvider,
                                      IConfiguration configuration,
                                      ILoggerFactory loggerFactory)
            : base(connection, options, dataProtectionProvider, configuration, loggerFactory)
        { }

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