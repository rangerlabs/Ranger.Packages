using System;
using System.Collections.Generic;

namespace Ranger.RabbitMQ {
    public interface ICorrelationContext {
        Guid CorrelationContextId { get; }
        Guid ResourceId { get; }
        string UserId { get; }
        string TraceId { get; }
        string SpanContext { get; }
        string ConnectionId { get; }
        string Name { get; }
        string Origin { get; }
        string Resource { get; }
        string Culture { get; }
        DateTime CreatedAt { get; }
        int Retries { get; }

        IDictionary<string, object> ToStringifiedDictionary ();
    }
}