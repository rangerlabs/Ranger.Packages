using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient {
    public class TenantsClient : ApiClientBase, ITenantsClient {
        private readonly ILogger<TenantsClient> logger;

        public TenantsClient (string uri, ILogger<TenantsClient> logger) : base (uri, logger, "tenantsApi") {
            this.logger = logger;
        }

        public async Task<bool> ExistsAsync (string domain) {
            if (String.IsNullOrWhiteSpace (domain)) {
                throw new ArgumentException ($"{nameof(domain)} cannot be null or whitespace.");
            }

            var apiResponse = new InternalApiResponse ();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() => {
                return new HttpRequestMessage () {
                Method = HttpMethod.Get,
                RequestUri = new Uri (httpClient.BaseAddress, $"tenant/exists/{domain}")
                };
            });
            apiResponse = await SendAsync (httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode) {
                return true;
            } else {
                return false;
            }
        }

        public async Task<T> GetTenantAsync<T> (string domain) {
            if (String.IsNullOrWhiteSpace (domain)) {
                throw new ArgumentException ($"{nameof(domain)} cannot be null or whitespace.");
            }

            var apiResponse = new InternalApiResponse<T> ();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() => {
                return new HttpRequestMessage () {
                Method = HttpMethod.Get,
                RequestUri = new Uri (httpClient.BaseAddress, $"/tenant/{domain}")
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