using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{
    public static class HttpClientExtensions
    {
        public static async Task SetClientToken(this HttpClient httpClient, ILogger logger, string scope, string clientId, string clientSecret)
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
                ClientId = clientId,
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