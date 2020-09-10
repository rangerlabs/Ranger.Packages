using System;
using RabbitMQ.Client;

namespace Ranger.RabbitMQ.BusPublisher
{
    public class RangerPublishExceptionData
    {
        public RangerPublishExceptionData(Type messageType, IBasicProperties basicProperties, string body)
        {
            this.MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
            this.BasicProperties = basicProperties;
            this.Body = body;

        }
        public Type MessageType { get; }
        public IBasicProperties BasicProperties { get; }
        public string Body { get; }
    }
}