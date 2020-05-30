using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ranger.RabbitMQ
{
    public class BusPublisher : IBusPublisher
    {
        private readonly ILogger<BusPublisher> logger;
        private readonly IModel channel;
        private readonly RabbitMQOptions options;
        private readonly Dictionary<Type, TopologyNames> topologyDictionary = new Dictionary<Type, TopologyNames>();

        public BusPublisher(ILogger<BusPublisher> logger, IConnection connection, RabbitMQOptions options)
        {
            this.logger = logger;
            this.options = options;
            channel = connection.CreateModel();
            channel.ConfirmSelect();
            logger.LogInformation("Publisher connected");
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

        private void ErrorPublish<TMessage>(TMessage message, BasicDeliverEventArgs ea) where TMessage : IMessage
        {
            TopologyNames topologyNames = TopologyForMessageType(message.GetType());
            channel.Bind<TMessage>(topologyNames.ErrorExchange, topologyNames.ErrorQueue, topologyNames.ErrorRoutingKey, options);

            try
            {
                this.channel.BasicPublish(topologyNames.ErrorExchange, topologyNames.ErrorRoutingKey.Replace("#.", ""), true, ea.BasicProperties, ea.Body);
                this.channel.WaitForConfirmsOrDie();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to publish message: '{message.GetType()}'with correlation id: '{CorrelationContext.IdFromBasicDeliverEventArgsHeader(ea).ToString()}'");
                throw;
            }
        }

        private void ChannelPublish<TMessage>(TMessage message, ICorrelationContext context = null) where TMessage : IMessage
        {
            TopologyNames topologyNames = TopologyForMessageType(message.GetType());

            this.channel.ExchangeDeclare(
                topologyNames.Exchange,
                ExchangeType.Topic,
                true);

            var messageContent = String.Empty;
            try
            {
                messageContent = JsonConvert.SerializeObject(message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to serialize object of type: '{message.GetType()}'");
                throw;
            }
            try
            {
                IBasicProperties messageProperties = CreateMessageHeaders(this.channel, context);
                logger.LogDebug($"Publishing message to exchange '{topologyNames.Exchange}' with routingkey '{topologyNames.RoutingKey.Replace("#.", "")}'");
                this.channel.BasicPublish(topologyNames.Exchange, topologyNames.RoutingKey.Replace("#.", ""), basicProperties: messageProperties, body: new ReadOnlyMemory<byte>(System.Text.Encoding.Default.GetBytes(messageContent)));
                this.channel.WaitForConfirmsOrDie();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to publish message: '{message.GetType()}'with correlation id: '{context.CorrelationContextId}'");
                throw;
            }
        }

        private IBasicProperties CreateMessageHeaders(IModel channel, ICorrelationContext context)
        {
            if (channel is null)
            {
                throw new ArgumentException(nameof(channel) + "was null");
            }
            if (context is null)
            {
                throw new ArgumentException(nameof(context) + "was null");
            }

            var messageProperties = channel.CreateBasicProperties();
            messageProperties.Persistent = true;
            messageProperties.Headers = context.ToStringifiedDictionary();

            return messageProperties;
        }

        private TopologyNames TopologyForMessageType(Type type)
        {
            var topologyNames = new TopologyNames
            {
                Exchange = NamingConventions.ExchangeNamingConvention(type, options.Namespace),
                Queue = NamingConventions.QueueNamingConvention(type, options.Namespace),
                RoutingKey = NamingConventions.RoutingKeyConvention(type, options.Namespace),
                ErrorExchange = NamingConventions.ErrorExchangeNamingConvention(type, options.Namespace),
                ErrorQueue = NamingConventions.ErrorQueueNamingConvention(type, options.Namespace),
                ErrorRoutingKey = NamingConventions.ErrorRoutingKeyConvention(type, options.Namespace),
            };
            try
            {
                topologyDictionary.Add(type, topologyNames);
            }
            catch (ArgumentException)
            {
                logger.LogDebug("The type {Type} is already entered into the topology dictionary", type.FullName);
            }
            return topologyNames;
        }

        private struct TopologyNames
        {
            public string Exchange { get; set; }
            public string Queue { get; set; }
            public string RoutingKey { get; set; }
            public string ErrorExchange { get; set; }
            public string ErrorQueue { get; set; }
            public string ErrorRoutingKey { get; set; }
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
            logger.LogDebug("BusPublisher.Dispose() called");
            Dispose(true);
        }
    }
}