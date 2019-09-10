using RabbitMQ.Client;

namespace Ranger.RabbitMQ
{
    internal static class TopologyBinder
    {
        internal static void Bind<TMessage>(this IModel channel, string exchangeName, string queueName, string routingKey, RabbitMQOptions options) where TMessage : IMessage
        {

            channel.ExchangeDeclare(
                exchangeName,
                ExchangeType.Topic,
                true);
            channel.QueueDeclare(
                queueName,
                true,
                false,
                false
            );
            channel.QueueBind(
                queueName,
                exchangeName,
                routingKey
            );
        }
    }
}