using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ranger.InternalHttpClient {
    public class TenantsClient : ITenantsClient {

        private readonly HttpClient httpClient;
        private readonly ILogger<TenantsClient> logger;
        public TenantsClient (string uri, ILogger<TenantsClient> logger) {
            httpClient = new HttpClient ();
            httpClient.BaseAddress = new Uri (uri);
            this.logger = logger;
        }
        public async Task SetClientToken () {

            // discover endpoints from metadata
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

        public async Task<InternalApiResponse<bool>> ExistsAsync (string domain) {
            var apiResponse = new InternalApiResponse<bool> ();
            Uri uri = new Uri (httpClient.BaseAddress, $"tenant/exists?domain={domain}");
            var response = await httpClient.GetAsync (uri);

            if (response.IsSuccessStatusCode) {
                var existsContent = await response.Content.ReadAsStringAsync ();
                apiResponse.IsSuccessStatusCode = true;
                apiResponse.StatusCode = response.StatusCode;
                apiResponse.ResponseObject = Boolean.Parse (existsContent);

            } else {
                var errorContent = await response.Content.ReadAsStringAsync ();
                apiResponse.IsSuccessStatusCode = false;
                apiResponse.StatusCode = response.StatusCode;
                apiResponse.Errors = JsonConvert.DeserializeObject<IEnumerable<string>> (errorContent);
            }

            return apiResponse;
        }

        public async Task<InternalApiResponse<T>> GetTenantAsync<T> (string domain) {
            var apiResponse = new InternalApiResponse<T> ();
            Uri uri = new Uri (httpClient.BaseAddress, $"/tenant?domain={domain}");
            var response = await httpClient.GetAsync (uri);

            if (response.IsSuccessStatusCode) {
                var tenantContent = await response.Content.ReadAsStringAsync ();
                apiResponse.IsSuccessStatusCode = true;
                apiResponse.StatusCode = response.StatusCode;
                apiResponse.ResponseObject = JsonConvert.DeserializeObject<T> (tenantContent);
            } else {
                var errorContent = await response.Content.ReadAsStringAsync ();
                apiResponse.IsSuccessStatusCode = false;
                apiResponse.StatusCode = response.StatusCode;
                apiResponse.Errors = JsonConvert.DeserializeObject<IEnumerable<string>> (errorContent);
            }

            return apiResponse;
        }
    }
}