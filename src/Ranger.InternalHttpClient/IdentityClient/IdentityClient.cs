using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using IdentityServer4;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ranger.InternalHttpClient {
    public class IdentityClient : IIdentityClient {
        private readonly HttpClient httpClient;
        private readonly ILogger<IdentityClient> logger;
        public IdentityClient (string uri, ILogger<IdentityClient> logger) {
            httpClient = new HttpClient ();
            httpClient.BaseAddress = new Uri (uri);
            this.logger = logger;
        }

        public async Task SetClientToken () {
            var disco = await httpClient.GetDiscoveryDocumentAsync (new DiscoveryDocumentRequest {
                Address = "http://identity_api:5000",
                    Policy = {
                        RequireHttps = false,
                        ValidateIssuerName = false
                    }
            });
            if (disco.IsError) {
                throw new Exception ("Failed to get discovery document for http client.", new Exception (disco.Error));
            }

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync (new ClientCredentialsTokenRequest {
                Address = disco.TokenEndpoint,

                    ClientId = "internal",
                    ClientSecret = "identitySecret",
                    Scope = IdentityServerConstants.LocalApi.ScopeName
            });

            if (tokenResponse.IsError) {
                throw new Exception ("Error with the token response.", tokenResponse.Exception);
            }

            logger.LogDebug ("Recieved token from identity server.");
            httpClient.SetBearerToken (tokenResponse.AccessToken);
        }

        public async Task<InternalApiResponse<T>> GetUserAsync<T> (string username) {
            var apiResponse = new InternalApiResponse<T> ();
            Uri uri = new Uri (httpClient.BaseAddress, $"user?username={username}");
            var response = await httpClient.GetAsync (uri);

            var userContent = await response.Content.ReadAsStringAsync ();
            if (response.IsSuccessStatusCode) {
                var roleContent = await response.Content.ReadAsStringAsync ();
                apiResponse.IsSuccessStatusCode = true;
                apiResponse.StatusCode = response.StatusCode;
                apiResponse.ResponseObject = JsonConvert.DeserializeObject<T> (userContent);
            } else {
                var errorContent = await response.Content.ReadAsStringAsync ();
                apiResponse.IsSuccessStatusCode = false;
                apiResponse.StatusCode = response.StatusCode;
                apiResponse.Errors = JsonConvert.DeserializeObject<IEnumerable<string>> (errorContent);
            }
            return apiResponse;

        }

        public async Task<InternalApiResponse<T>> GetAllUsersAsync<T> () {
            var apiResponse = new InternalApiResponse<T> ();
            Uri uri = new Uri (httpClient.BaseAddress, "user/all");
            var response = await httpClient.GetAsync (uri);

            var usersContent = await response.Content.ReadAsStringAsync ();
            if (response.IsSuccessStatusCode) {
                var roleContent = await response.Content.ReadAsStringAsync ();
                apiResponse.IsSuccessStatusCode = true;
                apiResponse.StatusCode = response.StatusCode;
                apiResponse.ResponseObject = JsonConvert.DeserializeObject<T> (usersContent);
            } else {
                var errorContent = await response.Content.ReadAsStringAsync ();
                apiResponse.IsSuccessStatusCode = false;
                apiResponse.StatusCode = response.StatusCode;
                apiResponse.Errors = JsonConvert.DeserializeObject<IEnumerable<string>> (errorContent);
            }
            return apiResponse;
        }

        public async Task<InternalApiResponse> CreateUser (ApplicationUserRequestModel user) {
            var apiResponse = new InternalApiResponse ();
            Uri uri = new Uri (httpClient.BaseAddress, "user");
            var userContent = new StringContent (JsonConvert.SerializeObject (user), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync (uri, userContent);
            if (response.IsSuccessStatusCode) {
                var roleContent = await response.Content.ReadAsStringAsync ();
                apiResponse.IsSuccessStatusCode = true;
                apiResponse.StatusCode = response.StatusCode;
            } else {
                var errorContent = await response.Content.ReadAsStringAsync ();
                apiResponse.IsSuccessStatusCode = false;
                apiResponse.StatusCode = response.StatusCode;
                apiResponse.Errors = JsonConvert.DeserializeObject<IEnumerable<string>> (errorContent);
            }
            return apiResponse;
        }

        public async Task<InternalApiResponse<T>> GetRoleAsync<T> (string name) {
            var apiResponse = new InternalApiResponse<T> ();
            Uri uri = new Uri (httpClient.BaseAddress, $"role?name={name}");
            var response = await httpClient.GetAsync (uri);
            if (response.IsSuccessStatusCode) {
                var roleContent = await response.Content.ReadAsStringAsync ();
                apiResponse.IsSuccessStatusCode = true;
                apiResponse.StatusCode = response.StatusCode;
                apiResponse.ResponseObject = JsonConvert.DeserializeObject<T> (roleContent);
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