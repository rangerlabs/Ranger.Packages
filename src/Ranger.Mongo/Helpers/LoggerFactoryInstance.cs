using Microsoft.Extensions.Logging;

namespace Ranger.Mongo
{
    public static class LoggerFactoryInstance
    {
        internal static ILoggerFactory Instance = new LoggerFactory();
    }
}