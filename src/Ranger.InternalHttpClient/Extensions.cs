using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace Ranger.InternalHttpClient
{
    public static class Extensions
    {
        public static IServiceCollection AddTenantsHttpClient(this IServiceCollection services, string baseAddress, string scope, string clientSecret)
            => AddHttpClient<TenantsHttpClient>(services, baseAddress, scope, clientSecret);
        public static IServiceCollection AddProjectsHttpClient(this IServiceCollection services, string baseAddress, string scope, string clientSecret)
            => AddHttpClient<ProjectsHttpClient>(services, baseAddress, scope, clientSecret);
        public static IServiceCollection AddSubscriptionsHttpClient(this IServiceCollection services, string baseAddress, string scope, string clientSecret)
            => AddHttpClient<SubscriptionsHttpClient>(services, baseAddress, scope, clientSecret);

        static IAsyncPolicy<HttpResponseMessage> ExponentialBackoffWithJitterPolicy()
        {
            Random jitterer = new Random();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 100)));
        }

        static IAsyncPolicy<HttpResponseMessage> RetryUnauthorizedPolicy(HttpClient httpClient, ILogger logger, string scope, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException($"{nameof(scope)} was null or whitespace");
            }

            return Policy
            .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.Unauthorized)
            .RetryAsync(1, async (exception, retryCount) =>
            {
                await httpClient.SetClientToken(logger, scope, clientSecret);
            });
        }

        static IAsyncPolicy<HttpResponseMessage> NoOpPolicy => Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>();

        static IServiceCollection AddHttpClient<T>(IServiceCollection services, string baseAddress, string scope, string clientSecret)
            where T : ApiClientBase
        {
            if (string.IsNullOrWhiteSpace(baseAddress))
            {
                throw new System.ArgumentException($"{nameof(baseAddress)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new System.ArgumentException($"{nameof(scope)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new System.ArgumentException($"{nameof(clientSecret)} was null or whitespace");
            }
            services.AddHttpClient<TenantsHttpClient>(client =>
            {
                client.BaseAddress = new Uri(baseAddress);
            })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
                .AddPolicyHandler((serviceProvider, request) =>
                    RetryUnauthorizedPolicy(
                        serviceProvider.GetService<T>().HttpClient,
                        serviceProvider.GetService<LoggerFactory>().CreateLogger("Ranger.InternalHttpClient.Extensions"),
                        scope,
                        clientSecret
                    ))
                .AddPolicyHandler(request => request.Method == HttpMethod.Get ? ExponentialBackoffWithJitterPolicy() : NoOpPolicy);

            return services;
        }

        static async Task SetClientToken(this HttpClient httpClient, ILogger logger, string scope, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException($"{nameof(scope)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentException($"{nameof(clientSecret)} was null or whitespace");
            }

            DiscoveryDocumentRequest discoveryDocument = null;
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
            {
                logger.LogDebug("Requesting discovery document for Development Environment");
                discoveryDocument = new DiscoveryDocumentRequest()
                {
                    Address = "http://identity:5000",
                    Policy = {
                        RequireHttps = false,
                        Authority = "http://localhost.io:5000",
                        ValidateEndpoints = false
                    },
                };
            }
            else
            {
                logger.LogDebug("Requesting discovery document for Production Environment");
                discoveryDocument = new DiscoveryDocumentRequest()
                {
                    Address = "http://identity:5000",
                    Policy = {
                        RequireHttps = false,
                        Authority = "https://rangerlabs.io",
                        ValidateEndpoints = false
                    },
                };

            }
            var disco = await httpClient.GetDiscoveryDocumentAsync(discoveryDocument);
            if (disco.IsError)
            {
                throw new Exception(disco.Error);
            }
            logger.LogDebug("Retrieved discovery document from Identity Server");

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "internal",
                ClientSecret = clientSecret,
                Scope = scope
            });

            if (tokenResponse.IsError)
            {
                throw new Exception("Error with the token response.", tokenResponse.Exception);
            }

            logger.LogDebug("Recieved token from identity server.");
            httpClient.SetBearerToken(tokenResponse.AccessToken);
        }
    }
}