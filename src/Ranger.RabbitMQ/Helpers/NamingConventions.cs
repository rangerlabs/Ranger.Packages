using System;
using System.Linq;
using System.Reflection;
using Ranger.Common;

namespace Ranger.RabbitMQ
{
    public static class NamingConventions
    {
        public static string ExchangeNamingConvention(Type type, string defaultNamespace) => GetNamespace(type, defaultNamespace).ToLowerInvariant();
        public static string RoutingKeyConvention(Type type, string defaultNamespace) => $"#.{GetRoutingKeyNamespace(type, defaultNamespace)}.{type.Name.Underscore()}".ToLowerInvariant();
        // public static string RoutingKeyConvention(Type type, string defaultNamespace) => $"#.{type.Name.Underscore()}".ToLowerInvariant();
        public static string QueueNamingConvention(Type type, string defaultNamespace) => GetQueueName(type, defaultNamespace);
        public static string ErrorExchangeNamingConvention(Type type, string defaultNamespace) => GetNamespace(type, defaultNamespace).ToLowerInvariant();
        public static string ErrorQueueNamingConvention(Type type, string defaultNamespace) => GetQueueName(type, defaultNamespace) + ".error";
        public static string ErrorRoutingKeyConvention(Type type, string defaultNamespace) => $"#.{GetRoutingKeyNamespace(type, defaultNamespace)}.{type.Name.Underscore()}".ToLowerInvariant() + ".error";
        // public static string ErrorRoutingKeyConvention(Type type, string defaultNamespace) => $"#.{type.Name.Underscore()}".ToLowerInvariant() + ".error";

        private static string GetNamespace(Type type, string defaultNamespace)
        {
            var @namespace = type.GetCustomAttribute<MessageNamespaceAttribute>()?.Namespace ?? defaultNamespace;
            return string.IsNullOrWhiteSpace(@namespace) ? type.Name.Underscore() : $"{@namespace}";
        }

        private static string GetQueueName(Type type, string defaultNamespace)
        {
            var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
            var consumerName = assemblyName.Split('.').ToList().Last();
            var @namespace = type.GetCustomAttribute<MessageNamespaceAttribute>()?.Namespace ?? defaultNamespace;
            var separatedNamespace = string.IsNullOrWhiteSpace(@namespace) ? string.Empty : $"{@namespace}";

            var messageType = "unknown";
            if (type.GetInterfaces().Contains(typeof(ICommand)))
            {
                messageType = "command";
            }
            else if (type.GetInterfaces().Contains(typeof(IRejectedEvent)))
            {
                messageType = "rejected_event";
            }
            else if (type.GetInterfaces().Contains(typeof(IEvent)))
            {
                messageType = "event";
            }

            return $"{consumerName}.{messageType}.{separatedNamespace}.{type.Name.Underscore()}".ToLowerInvariant();
        }

        private static string GetRoutingKeyNamespace(Type type, string defaultNamespace)
        {
            var @namespace = type.GetCustomAttribute<MessageNamespaceAttribute>()?.Namespace ?? defaultNamespace;
            return string.IsNullOrWhiteSpace(@namespace) ? string.Empty : $"{@namespace}";
        }
    }
}