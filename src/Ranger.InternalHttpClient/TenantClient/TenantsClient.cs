using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;

namespace Ranger.InternalHttpClient {
    public class TenantsClient : ApiClientBase<TenantsClient>, ITenantsClient {

        private readonly ILogger<TenantsClient> logger;
        public TenantsClient (string uri, ILogger<TenantsClient> logger) : base (uri, logger) { }

        public async Task<InternalApiResponse> ExistsAsync (string domain) {
            var apiResponse = new InternalApiResponse ();
            var httpRequestMessage = new HttpRequestMessage () {
                Method = HttpMethod.Get,
                RequestUri = new Uri (httpClient.BaseAddress, $"tenant/exists?domain={domain}")
            };
            return await SendAsync (httpRequestMessage);
        }

        public async Task<InternalApiResponse<T>> GetTenantAsync<T> (string domain) {
            var apiResponse = new InternalApiResponse<T> ();
            var httpRequestMessage = new HttpRequestMessage () {
                Method = HttpMethod.Get,
                RequestUri = new Uri (httpClient.BaseAddress, $"/tenant?domain={domain}")
            };
            return await SendAsync<T> (httpRequestMessage);
        }
    }
}