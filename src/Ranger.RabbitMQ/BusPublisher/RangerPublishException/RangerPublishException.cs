using System;
using System.Runtime.Serialization;

namespace Ranger.RabbitMQ.BusPublisher
{
    public class RangerPublishException : Exception
    {
        public RangerPublishException()
        {
        }

        public RangerPublishException(string message) : base(message)
        {
        }

        public RangerPublishException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RangerPublishException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}