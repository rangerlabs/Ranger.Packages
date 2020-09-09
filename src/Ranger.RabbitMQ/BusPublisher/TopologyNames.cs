using Microsoft.EntityFrameworkCore;

namespace Ranger.RabbitMQ.BusPublisher
{
    public struct TopologyNames
    {
        public string Exchange { get; set; }
        public string Queue { get; set; }
        public string RoutingKey { get; set; }
        public string ErrorExchange { get; set; }
        public string ErrorQueue { get; set; }
        public string ErrorRoutingKey { get; set; }
    }
}