using RabbitMQ.Client;

namespace Ranger.RabbitMQ {
    internal static class TopologyBinder {
        internal static void Bind<TMessage> (this IModel channel, string exchangeName, string queueName, RabbitMQOptions options) where TMessage : IMessage {
            var routingKey = NamingConventions.RoutingKeyConvention (typeof (TMessage), options.Namespace);

            channel.ExchangeDeclare (
                exchangeName,
                ExchangeType.Topic,
                true);
            channel.QueueDeclare (
                queueName,
                true,
                false,
                false
            );
            channel.QueueBind (
                queueName,
                exchangeName,
                routingKey
            );
        }
    }
}