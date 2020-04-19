using System.Net.Http;
using Microsoft.Extensions.Logging;
using Polly;

namespace Ranger.InternalHttpClient
{
    internal static class PollyContextExtensions
    {
        private static readonly string LoggerKey = "ILogger";
        private static readonly string HttpClientOptionsKey = "HttpClientOptions";
        private static readonly string HttpClientKey = "HttpClient";

        public static Context WithLogger(this Context context, ILogger logger)
        {
            context[LoggerKey] = logger;
            return context;
        }

        public static ILogger GetLogger(this Context context)
        {
            if (context.TryGetValue(LoggerKey, out var logger))
            {
                return logger as ILogger;
            }

            return null;
        }

        public static Context WithHttpClientOptions(this Context context, IHttpClientOptions options)
        {
            context[HttpClientOptionsKey] = options;
            return context;
        }

        public static IHttpClientOptions GetHttpClientOptions(this Context context)
        {
            if (context.TryGetValue(HttpClientOptionsKey, out var options))
            {
                return options as IHttpClientOptions;
            }
            return null;
        }

        public static Context WithHttpClient(this Context context, HttpClient httpClient)
        {
            context[HttpClientKey] = httpClient;
            return context;
        }

        public static HttpClient GetHttpClient(this Context context)
        {
            if (context.TryGetValue(HttpClientKey, out var httpClient))
            {
                return httpClient as HttpClient;
            }
            return null;
        }
    }
}