using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace Ranger.InternalHttpClient
{
    internal class PollyPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> AuthorizationRetryPolicy =>
            Policy
            .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.Unauthorized)
            .RetryAsync(1, async (exception, retryCount, context) =>
            {
                var logger = context.GetLogger();
                var httpClient = context.GetHttpClient();
                var options = context.GetHttpClientOptions();

                logger.LogInformation("Requesting new access token");
                await httpClient.SetClientToken(logger, options.Scope, options.ClientId, options.ClientSecret);
                logger.LogInformation("New access token acquired");
            });

        public static IAsyncPolicy<HttpResponseMessage> ExponentialBackoffWithJitterPolicy()
        {
            Random jitterer = new Random();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(4, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 100)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        context.GetLogger()?.LogWarning("Delaying for {delay}ms before making retry {retry}", timespan.TotalMilliseconds, retryAttempt);
                    });
        }
        public static IAsyncPolicy<HttpResponseMessage> NoOpPolicy => Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>();
    }
}