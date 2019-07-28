using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using Ranger.Common;

namespace Ranger.RabbitMQ {
    public class CorrelationContext : ICorrelationContext {
        private static ILogger<CorrelationContext> logger = LoggerFactoryInstance.Instance.CreateLogger<CorrelationContext> ();
        public Guid CorrelationContextId { get; private set; } = Guid.Empty;
        public Guid ResourceId { get; private set; } = Guid.Empty;
        public string UserId { get; private set; } = "";
        public string TraceId { get; private set; } = "";
        public string SpanContext { get; private set; } = "";
        public string ConnectionId { get; private set; } = "";
        public string Name { get; private set; } = "";
        public string Origin { get; private set; } = "";
        public string Resource { get; private set; } = "";
        public string Culture { get; private set; } = "";
        public DateTime CreatedAt { get; private set; } = DateTime.MinValue;
        public int Retries { get; internal set; } = 0;

        public CorrelationContext () { }

        private CorrelationContext (Guid correlationContextId) {
            CorrelationContextId = correlationContextId;
        }

        [JsonConstructor]
        private CorrelationContext (Guid correlationContextId, string userId, Guid resourceId, string traceId, string spanContext, string connectionId, string name, string origin, string culture, string resource, int retries) {
            CorrelationContextId = correlationContextId;
            UserId = userId;
            ResourceId = resourceId;
            TraceId = traceId;
            SpanContext = spanContext;
            ConnectionId = connectionId;
            Name = string.IsNullOrWhiteSpace (name) ? string.Empty : GetName (name);
            Origin = string.IsNullOrWhiteSpace (origin) ? string.Empty : origin.StartsWith ("/") ? origin.Remove (0, 1) : origin;
            Culture = culture;
            Resource = resource;
            Retries = retries;
            CreatedAt = DateTime.UtcNow;
        }

        public static ICorrelationContext Empty => new CorrelationContext ();

        public static ICorrelationContext FromId (Guid correlationContextId) => new CorrelationContext (correlationContextId: correlationContextId);

        public static ICorrelationContext From<T> (ICorrelationContext context) {
            return Create<T> (context.CorrelationContextId, context.UserId, context.ResourceId, context.TraceId, context.ConnectionId, context.Origin, context.Culture, context.Resource);
        }

        public static ICorrelationContext Create<T> (Guid correlationContextId, string userId, Guid resourceId, string origin, string traceId, string spanContext, string connectionId, string culture, string resource = "") {
            return new CorrelationContext (correlationContextId, userId, resourceId, traceId, spanContext, connectionId, typeof (T).Name, origin, culture, resource, 0);
        }

        public static Guid IdFromBasicDeliverEventArgsHeader (BasicDeliverEventArgs ea) {
            object correlationContextId = null;
            bool correlationContextIdAvailable = ea.BasicProperties.Headers.TryGetValue ("CorrelationContextId", out correlationContextId);
            return correlationContextIdAvailable ? Guid.Parse (correlationContextId as string) : Guid.Empty;
        }

        public static CorrelationContext FromBasicDeliverEventArgs (BasicDeliverEventArgs ea) {
            var context = new CorrelationContext ();
            foreach (var propertyInfo in context.GetType ().GetProperties (BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)) {
                object headerValue = default (object);
                bool headerValueAvailable = ea.BasicProperties.Headers.TryGetValue (propertyInfo.Name, out headerValue);
                if (headerValueAvailable) {
                    var stringValue = System.Text.Encoding.Default.GetString (headerValue as byte[]);
                    if (!String.IsNullOrWhiteSpace (stringValue)) {
                        if (propertyInfo.PropertyType == typeof (Guid)) {
                            var guidValue = Guid.Parse (stringValue);
                            propertyInfo.SetValue (context, guidValue);
                        } else if (propertyInfo.PropertyType == typeof (DateTime)) {
                            DateTime dateTimeValue = DateTime.Parse (stringValue);
                            propertyInfo.SetValue (context, dateTimeValue);
                        } else if (propertyInfo.PropertyType == typeof (Int32)) {
                            Int32 intValue = Int32.Parse (stringValue);
                            propertyInfo.SetValue (context, intValue);
                        } else {
                            propertyInfo.SetValue (context, stringValue);
                        }
                    }
                }
            }
            return context;
        }

        public IDictionary<string, object> ToStringifiedDictionary () {
            var dictionary = new Dictionary<string, object> ();
            foreach (var propertyInfo in this.GetType ().GetProperties (BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)) {
                dictionary.Add (propertyInfo.Name, propertyInfo.GetValue (this).ToString ());
            }
            return dictionary;
        }

        private static string GetName (string name) => name.Underscore ().ToLowerInvariant ();

        internal void IncrementRetries () => this.Retries++;
    }
}