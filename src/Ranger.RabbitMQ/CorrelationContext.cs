using System;

namespace Ranger.RabbitMQ {
    public class CorrelationContext {
        public Guid Id { get; }
        public Guid UserId { get; }
        public Guid ResourceId { get; }
        public string TraceId { get; }
        public string SpanContext { get; }
        public string ConnectionId { get; }
        public string Name { get; }
        public string Origin { get; }
        public string Resource { get; }
        public string Culture { get; }
        public DateTime CreatedAt { get; }
        public int Retries { get; }

        public CorrelationContext (Guid id, Guid userId, Guid resourceId, string traceId, string spanContext, string connectionId, string name, string origin, string resource, string culture, DateTime createdAt, int retries) {
            this.Id = id;
            this.UserId = userId;
            this.ResourceId = resourceId;
            this.TraceId = traceId;
            this.SpanContext = spanContext;
            this.ConnectionId = connectionId;
            this.Name = name;
            this.Origin = origin;
            this.Resource = resource;
            this.Culture = culture;
            this.CreatedAt = createdAt;
            this.Retries = retries;
        }

        public void IncrementRetries => this.Retries++;
    }
}