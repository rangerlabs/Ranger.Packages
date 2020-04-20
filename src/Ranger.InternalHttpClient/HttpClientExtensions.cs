using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace Ranger.InternalHttpClient
{
    public static class HttpClientExtensions
    {
        public static async Task SetNewClientToken(this HttpRequestMessage httpRequestMessage, HttpClient httpClient, IHttpClientOptions options, ILogger logger)
        {
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
                logger.LogCritical(disco.Exception, "Error with the discovery document response");
                throw new ApiException("An internal server error occurred", StatusCodes.Status500InternalServerError);
            }
            logger.LogDebug("Retrieved discovery document from Identity Server");

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = options.ClientId,
                ClientSecret = options.ClientSecret,
                Scope = options.Scope
            });
            if (tokenResponse.IsError)
            {
                logger.LogCritical(tokenResponse.Exception, "Error with the token response");
                throw new ApiException("An internal server error occurred", StatusCodes.Status500InternalServerError);
            }

            httpRequestMessage.SetBearerToken(tokenResponse.AccessToken);
            options.Token = tokenResponse.AccessToken;
            logger.LogDebug("Received new access token from Identity Server");
        }
    }
}