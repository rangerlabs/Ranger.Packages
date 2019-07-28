using Microsoft.Extensions.Logging;

namespace Ranger.RabbitMQ {
    public static class LoggerFactoryInstance {
        internal static ILoggerFactory Instance = new LoggerFactory ();
    }
}