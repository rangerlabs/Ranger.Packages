using System;
using Ranger.RabbitMQ.BusPublisher;
using Xunit;
using Shouldly;
using System.Linq;

namespace Ranger.RabbitMq.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

            typeof(Msg).CustomAttributes.Select(a => a.AttributeType).Contains(typeof(NonAckedAttribute)).ShouldBeTrue();

        }
    }

    [NonAcked]
    public class Msg
    { }
}
