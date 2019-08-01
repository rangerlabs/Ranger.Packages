using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ranger.RabbitMQ {
    public class BusPublisher : IBusPublisher {
        private readonly ILogger<BusPublisher> logger;
        private readonly IConnectionFactory connectionFactory;
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly RabbitMQOptions options;
        private readonly Dictionary<Type, TopologyNames> topologyDictionary = new Dictionary<Type, TopologyNames> ();

        public BusPublisher (ILogger<BusPublisher> logger, IConnectionFactory connectionFactory, RabbitMQOptions options) {
            this.logger = logger;
            this.connectionFactory = connectionFactory;
            this.options = options;
            connection = connectionFactory.CreateConnection ();
            logger.LogInformation ("Publisher connected.");
            channel = connection.CreateModel ();

            channel.ConfirmSelect ();
        }

        public void Publish<TEvent> (TEvent @event, ICorrelationContext context = null) where TEvent : IEvent {
            ChannelPublish<TEvent> (@event, context);
        }

        public void Send<TCommand> (TCommand command, ICorrelationContext context = null) where TCommand : ICommand {
            ChannelPublish<TCommand> (command, context);
        }

        public void Error<TMessage> (TMessage message, BasicDeliverEventArgs ea) where TMessage : IMessage {
            ErrorPublish<TMessage> (message, ea);
        }

        private void ErrorPublish<TMessage> (TMessage message, BasicDeliverEventArgs ea) where TMessage : IMessage {
            TopologyNames topologyNames = TopologyForMessageType<TMessage> ();
            channel.Bind<TMessage> (topologyNames.ErrorExchange, topologyNames.ErrorQueue, options);

            try {
                this.channel.BasicPublish (topologyNames.ErrorExchange, topologyNames.ErrorRoutingKey.Replace ("#.", ""), true, ea.BasicProperties, ea.Body);
                this.channel.WaitForConfirmsOrDie ();
            } catch (Exception ex) {
                logger.LogError (ex, $"Failed to publish message: '{typeof(TMessage)}'" +
                    $"with correlation id: '{CorrelationContext.IdFromBasicDeliverEventArgsHeader(ea).ToString()}'");
                throw;
            }
        }

        private void ChannelPublish<TMessage> (TMessage message, ICorrelationContext context = null) where TMessage : IMessage {
            TopologyNames topologyNames = TopologyForMessageType<TMessage> ();

            this.channel.ExchangeDeclare (
                topologyNames.Exchange,
                ExchangeType.Topic,
                true);

            var messageContent = String.Empty;
            try {
                messageContent = JsonConvert.SerializeObject (message);
            } catch (Exception ex) {
                logger.LogError (ex, $"Failed to serialize object of type: '{typeof(TMessage)}'.");
                throw;
            }
            try {
                IBasicProperties messageProperties = CreateMessageHeaders (this.channel, context);
                this.channel.BasicPublish (topologyNames.Exchange, topologyNames.RoutingKey.Replace ("#.", ""), basicProperties : messageProperties, body : System.Text.Encoding.Default.GetBytes (messageContent));
                this.channel.WaitForConfirmsOrDie ();
            } catch (Exception ex) {
                logger.LogError (ex, $"Failed to publish message: '{typeof(TMessage)}'" +
                    $"with correlation id: '{context.CorrelationContextId}'");
                throw;
            }
        }

        private IBasicProperties CreateMessageHeaders (IModel channel, ICorrelationContext context) {
            if (channel is null) {
                throw new ArgumentException (nameof (channel) + "was null.");
            }
            if (context is null) {
                throw new ArgumentException (nameof (context) + "was null.");
            }

            var messageProperties = channel.CreateBasicProperties ();
            messageProperties.Persistent = true;
            messageProperties.Headers = context.ToStringifiedDictionary ();

            return messageProperties;
        }

        private TopologyNames TopologyForMessageType<TMessage> () where TMessage : IMessage {
            if (!topologyDictionary.ContainsKey (typeof (TMessage))) {
                topologyDictionary.Add (typeof (TMessage), new TopologyNames () {
                    Exchange = NamingConventions.ExchangeNamingConvention (typeof (TMessage), options.Namespace),
                        Queue = NamingConventions.QueueNamingConvention (typeof (TMessage), options.Namespace),
                        RoutingKey = NamingConventions.RoutingKeyConvention (typeof (TMessage), options.Namespace),
                        ErrorExchange = NamingConventions.ErrorExchangeNamingConvention (typeof (TMessage), options.Namespace),
                        ErrorQueue = NamingConventions.ErrorQueueNamingConvention (typeof (TMessage), options.Namespace),
                        ErrorRoutingKey = NamingConventions.RoutingKeyConvention (typeof (TMessage), options.Namespace),
                });
            }
            var topologyNames = topologyDictionary[typeof (TMessage)];
            return topologyNames;
        }

        private struct TopologyNames {
            public string Exchange { get; set; }
            public string Queue { get; set; }
            public string RoutingKey { get; set; }
            public string ErrorExchange { get; set; }
            public string ErrorQueue { get; set; }
            public string ErrorRoutingKey { get; set; }
        }

        private bool disposedValue = false;
        protected virtual void Dispose (bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    channel.Close ();
                    channel.Dispose ();
                    connection.Close ();
                    connection.Dispose ();
                }

                disposedValue = true;
            }
        }

        public void Dispose () {
            logger.LogDebug ("BusPublisher.Dispose() called.");
            Dispose (true);
        }
    }
}