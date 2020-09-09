using System;

namespace Ranger.RabbitMQ.BusPublisher
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NonAckedAttribute : Attribute
    {

    }
}