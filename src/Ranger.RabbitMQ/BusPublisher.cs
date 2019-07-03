using System;
using System.Collections.Generic;
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
        private readonly IBasicProperties messageProperties;
        private readonly RabbitMQOptions options;
        private readonly Dictionary<Type, TopologyNames> topologyDictionary = new Dictionary<Type, TopologyNames> ();

        public BusPublisher (ILogger<BusPublisher> logger, IConnectionFactory connectionFactory, RabbitMQOptions options) {
            this.logger = logger;
            this.connectionFactory = connectionFactory;
            this.options = options;
            connection = connectionFactory.CreateConnection ();
            logger.LogInformation ("Publisher connected.");
            channel = connection.CreateModel ();

            this.messageProperties = channel.CreateBasicProperties ();
            messageProperties.Persistent = true;
            channel.ConfirmSelect ();
        }

        public void PublishAsync<TEvent> (TEvent @event) where TEvent : IEvent {
            ChannelPublish<TEvent> (@event);
        }

        public void SendAsync<TCommand> (TCommand command) where TCommand : ICommand {
            ChannelPublish<TCommand> (command);
        }

        public void ErrorAsync<TMessage> (TMessage message, BasicDeliverEventArgs ea) where TMessage : IMessage {
            ErrorPublish<TMessage> (message, ea);
        }

        private void ErrorPublish<TMessage> (TMessage message, BasicDeliverEventArgs ea) where TMessage : IMessage {
            TopologyNames topologyNames = TopologyForMessageType<TMessage> ();
            channel.Bind<TMessage> (topologyNames.ErrorExchange, topologyNames.ErrorQueue, options);

            try {
                this.channel.BasicPublish (topologyNames.ErrorExchange, topologyNames.ErrorRoutingKey.Replace ("#.", ""), true, ea.BasicProperties, ea.Body);
                this.channel.WaitForConfirmsOrDie ();
            } catch (Exception ex) {
                logger.LogError ($"Failed to publish message: '{typeof(TMessage)}'" +
                    $"with correlation id: '{message.CorrelationContext.Id}'", ex);
                throw;
            }
        }

        private void ChannelPublish<TMessage> (TMessage message) where TMessage : IMessage {
            TopologyNames topologyNames = TopologyForMessageType<TMessage> ();

            channel.ExchangeDeclare (
                topologyNames.Exchange,
                ExchangeType.Topic,
                true);

            var messageContent = String.Empty;
            try {
                messageContent = JsonConvert.SerializeObject (message);
            } catch (Exception ex) {
                logger.LogError ($"Failed to serialize object of type: '{typeof(TMessage)}'.", ex);
                throw;
            }
            try {
                this.channel.BasicPublish (topologyNames.Exchange, topologyNames.RoutingKey.Replace ("#.", ""), basicProperties : this.messageProperties, body : System.Text.Encoding.Default.GetBytes (messageContent));
                this.channel.WaitForConfirmsOrDie ();
            } catch (Exception ex) {
                logger.LogError ($"Failed to publish message: '{message.GetType().Name}'" +
                    $"with correlation id: '{message.CorrelationContext.Id}'", ex);
                throw;
            }
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