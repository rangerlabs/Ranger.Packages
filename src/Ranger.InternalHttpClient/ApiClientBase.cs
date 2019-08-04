using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ranger.InternalHttpClient {
    public class ApiClientBase<T> : IApiClient
    where T : class {
        private readonly ILogger<T> logger;
        protected readonly HttpClient httpClient;

        public ApiClientBase (string uri, ILogger<T> logger) {
            httpClient = new HttpClient ();
            httpClient.BaseAddress = new Uri (uri);
            this.logger = logger;
        }

        public async Task SetClientToken () {

            var disco = await httpClient.GetDiscoveryDocumentAsync ("http://identity:5000");
            if (disco.IsError) {
                Console.WriteLine (disco.Error);
                return;
            }

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync (new ClientCredentialsTokenRequest {
                Address = disco.TokenEndpoint,

                    ClientId = "internal",
                    ClientSecret = "cKprgh9wYKWcsm",
                    Scope = "tenantsApi"
            });

            if (tokenResponse.IsError) {
                throw new Exception ("Error with the token response.", tokenResponse.Exception);
            }

            logger.LogDebug ("Recieved token from identity server.");
            httpClient.SetBearerToken (tokenResponse.AccessToken);
        }

        protected async Task<InternalApiResponse> SendAsync (Func<HttpRequestMessage> httpRequestMessageFactory) {
            var apiResponse = new InternalApiResponse ();
            var response = await httpClient.SendAsync (httpRequestMessageFactory ());
            if (response.StatusCode == HttpStatusCode.Unauthorized) {
                logger.LogInformation ("Recieved a 403 Unauthorized from requested service. Attempting to set new client token.");
                await SetClientToken ();
                response = await httpClient.SendAsync (httpRequestMessageFactory ());
            }
            if (response.IsSuccessStatusCode) {
                apiResponse.IsSuccessStatusCode = true;
                apiResponse.StatusCode = response.StatusCode;
            } else {
                apiResponse.IsSuccessStatusCode = false;
                apiResponse.StatusCode = response.StatusCode;
                var errorContent = await response.Content?.ReadAsStringAsync () ?? "";
                apiResponse.Errors = JsonConvert.DeserializeObject<IEnumerable<string>> (errorContent);
            }
            return apiResponse;

        }

        protected async Task<InternalApiResponse<TReponseObject>> SendAsync<TReponseObject> (Func<HttpRequestMessage> httpRequestMessageFactory) {
            var apiResponse = new InternalApiResponse<TReponseObject> ();
            var response = await httpClient.SendAsync (httpRequestMessageFactory ());
            if (response.StatusCode == HttpStatusCode.Unauthorized) {
                logger.LogDebug ("Recieved a 403 Unauthorized from requested service. Attempting to set new client token.");
                await SetClientToken ();
                logger.LogDebug ("New client token set. Resending request.");
                response = await httpClient.SendAsync (httpRequestMessageFactory ());
            }
            if (response.IsSuccessStatusCode) {
                apiResponse.IsSuccessStatusCode = true;
                apiResponse.StatusCode = response.StatusCode;
                var content = await response.Content?.ReadAsStringAsync () ?? "";
                if (!String.IsNullOrWhiteSpace (content)) {
                    try {
                        apiResponse.ResponseObject = JsonConvert.DeserializeObject<TReponseObject> (content);
                    } catch (JsonSerializationException ex) {
                        throw new Exception ($"Failed to deserialize object to type '{typeof(T)}'.", ex);
                    }
                } else {
                    throw new Exception ($"The response body was empty. Verify the requested method returns a response body. Did you intend to use the non-generic 'SendAsync(HttpRequestMessage)'?");
                }
            } else {
                apiResponse.IsSuccessStatusCode = false;
                apiResponse.StatusCode = response.StatusCode;
                var errorContent = await response.Content?.ReadAsStringAsync () ?? "";
                apiResponse.Errors = JsonConvert.DeserializeObject<IEnumerable<string>> (errorContent);
            }
            return apiResponse;
        }

    }
}