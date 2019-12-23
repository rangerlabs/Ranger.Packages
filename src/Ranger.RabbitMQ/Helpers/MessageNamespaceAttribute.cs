using System;
using Ranger.Common;

namespace Ranger.RabbitMQ
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageNamespaceAttribute : Attribute
    {
        public string Namespace { get; }

        public MessageNamespaceAttribute(string @namespace)
        {
            if (String.IsNullOrWhiteSpace(@namespace))
            {
                throw new RangerException("Message namesapce cannot be null or whitespace.");
            }
            Namespace = @namespace.ToLowerInvariant();
        }
    }
}