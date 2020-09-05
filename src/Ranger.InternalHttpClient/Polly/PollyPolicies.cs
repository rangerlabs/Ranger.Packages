using System;
using System.Net;
using System.Net.Http;
using AutoWrapper.Wrappers;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
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
                var httpClient = context.GetHttpClient();
                var requestMessage = context.GetHttpRequestMessage();
                var logger = context.GetLogger();
                var options = context.GetHttpClientOptions();
                logger.LogInformation("Recieved Status Code {StatusCode}, requesting new token", HttpStatusCode.Unauthorized);
                await requestMessage.SetNewClientToken(httpClient, options, logger);
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