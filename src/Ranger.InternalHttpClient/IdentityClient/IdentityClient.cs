using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient {
    public class IdentityClient : ApiClientBase, IIdentityClient {
        private readonly ILogger<IdentityClient> logger;

        public IdentityClient (string uri, ILogger<IdentityClient> logger) : base (uri, logger, IdentityServerConstants.LocalApi.ScopeName) {
            this.logger = logger;
        }

        public async Task<T> GetUserAsync<T> (string domain, string username) {
            if (String.IsNullOrWhiteSpace (domain)) {
                throw new ArgumentException ($"{nameof(domain)} cannot be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace (username)) {
                throw new ArgumentException ($"{nameof(domain)} cannot be null or whitespace.");
            }

            var apiResponse = new InternalApiResponse<T> ();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() => {
                return new HttpRequestMessage () {
                Method = HttpMethod.Get,
                RequestUri = new Uri (httpClient.BaseAddress, $"user/{username}"),
                Headers = { { "X-Tenant-Domain", domain },
                }
                };
            });
            apiResponse = await SendAsync<T> (httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode) {
                return apiResponse.ResponseObject;
            } else {
                throw GenericErrorException<T> (apiResponse);
            }
        }

        public async Task<T> GetAllUsersAsync<T> (string domain) {
            if (String.IsNullOrWhiteSpace (domain)) {
                throw new ArgumentException ($"{nameof(domain)} cannot be null or whitespace.");
            }

            var apiResponse = new InternalApiResponse<T> ();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() => {
                return new HttpRequestMessage () {
                Method = HttpMethod.Get,
                RequestUri = new Uri (httpClient.BaseAddress, $"user/all"),
                Headers = { { "X-Tenant-Domain", domain },
                }
                };
            });
            apiResponse = await SendAsync<T> (httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode) {
                return apiResponse.ResponseObject;
            } else {
                throw GenericErrorException<T> (apiResponse);
            }
        }

        public async Task<T> GetRoleAsync<T> (string name) {
            if (string.IsNullOrWhiteSpace (name)) {
                throw new ArgumentException ($"{nameof(name)} cannot be null or whitespace.");
            }

            var apiResponse = new InternalApiResponse<T> ();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() => {
                return new HttpRequestMessage () {
                Method = HttpMethod.Get,
                RequestUri = new Uri ($"role/{name}")
                };
            });
            apiResponse = await SendAsync<T> (httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode) {
                return apiResponse.ResponseObject;
            } else {
                throw GenericErrorException<T> (apiResponse);
            }
        }
    }
}