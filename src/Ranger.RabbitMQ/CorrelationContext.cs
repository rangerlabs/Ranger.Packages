using System;

namespace Ranger.RabbitMQ {
    public class CorrelationContext {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ResourceId { get; set; }
        public string TraceId { get; set; }
        public string SpanContext { get; set; }
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public string Origin { get; set; }
        public string Resource { get; set; }
        public string Culture { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Retries { get; set; }
    }
}