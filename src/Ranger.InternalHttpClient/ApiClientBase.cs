using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;

namespace Ranger.InternalHttpClient
{
    public class ApiClientBase : IApiClient
    {
        private readonly ILogger logger;
        private readonly string scope;
        protected readonly HttpClient httpClient;

        public ApiClientBase(string uri, ILogger logger, string scope)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException($"{nameof(scope)} cannot be null or whitespace");
            }

            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(uri);
            this.logger = logger;
            this.scope = scope;
        }

        public async Task SetClientToken()
        {
            DiscoveryDocumentRequest discoveryDocument = null;
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
            {
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

            //TODO: The client secret should be on a per client basis, for now just using a single one and this works because the this Identity Client has this and the other tokens approved.
            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "internal",
                ClientSecret = "cKprgh9wYKWcsm",
                Scope = scope
            });

            if (tokenResponse.IsError)
            {
                throw new Exception("Error with the token response.", tokenResponse.Exception);
            }

            logger.LogDebug("Recieved token from identity server.");
            httpClient.SetBearerToken(tokenResponse.AccessToken);
        }

        protected async Task<InternalApiResponse> SendAsync(Func<HttpRequestMessage> httpRequestMessageFactory)
        {
            logger.LogDebug("Executing SendAsync().");
            var apiResponse = new InternalApiResponse();
            var response = await httpClient.SendAsync(httpRequestMessageFactory());
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                logger.LogInformation("Recieved a 401 Unauthorized from requested service. Attempting to set new client token.");
                await SetClientToken();
                response = await httpClient.SendAsync(httpRequestMessageFactory());
            }
            if (response.IsSuccessStatusCode)
            {
                logger.LogDebug("Request was successful.");
                apiResponse.IsSuccessStatusCode = true;
                apiResponse.StatusCode = response.StatusCode;
            }
            else
            {
                logger.LogDebug("Request was unsuccessful.");
                apiResponse.IsSuccessStatusCode = false;
                apiResponse.StatusCode = response.StatusCode;
                var errorContent = await response.Content?.ReadAsStringAsync() ?? "";
                try
                {
                    apiResponse.Errors = JsonConvert.DeserializeObject<ApiErrorContent>(errorContent);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to deserialize the content of an error response. The response content may not have been a valid ApiErrorContent object.");
                    apiResponse.Errors = new ApiErrorContent();
                }
            }
            return apiResponse;

        }

        protected async Task<InternalApiResponse<TResponseObject>> SendAsync<TResponseObject>(Func<HttpRequestMessage> httpRequestMessageFactory)
        {
            var apiResponse = new InternalApiResponse<TResponseObject>();
            var response = await httpClient.SendAsync(httpRequestMessageFactory());
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                logger.LogDebug("Recieved a 401 Unauthorized from requested service. Attempting to set new client token.");
                await SetClientToken();
                logger.LogDebug("New client token set. Resending request.");
                response = await httpClient.SendAsync(httpRequestMessageFactory());
            }
            apiResponse.IsSuccessStatusCode = true;
            apiResponse.StatusCode = response.StatusCode;
            var content = await response.Content?.ReadAsStringAsync() ?? "";
            switch (response.StatusCode)
            {
                case HttpStatusCode.NoContent:
                    {

                        logger.LogDebug("Request was successful.");
                        apiResponse.ResponseObject = default;
                        break;
                    }
                case HttpStatusCode.OK:
                    {
                        logger.LogDebug("Request was successful.");
                        if (!String.IsNullOrWhiteSpace(content))
                        {
                            try
                            {
                                apiResponse.ResponseObject = JsonConvert.DeserializeObject<TResponseObject>(content);
                            }
                            catch (JsonSerializationException ex)
                            {
                                throw new Exception($"Failed to deserialize object to type '{typeof(TResponseObject)}'.", ex);
                            }
                        }
                        else
                        {
                            throw new Exception($"The response body was empty. Verify the requested method returns a response body. Did you intend to use the non-generic 'SendAsync(HttpRequestMessage)'?");
                        }
                        break;
                    }
                default:
                    {
                        logger.LogDebug("Request was unsuccessful.");
                        apiResponse.IsSuccessStatusCode = false;
                        try
                        {
                            apiResponse.Errors = JsonConvert.DeserializeObject<ApiErrorContent>(content);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to deserialize the content of an error response. The response content may not have been a valid ApiErrorContent object.");
                            apiResponse.Errors = new ApiErrorContent();
                        }
                        break;
                    }
            }
            return apiResponse;
        }
    }
}