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

        public TenantsClient (string uri, ILogger<TenantsClient> logger) : base (uri, logger) { }

        public async Task<bool> ExistsAsync (string domain) {
            var apiResponse = new InternalApiResponse ();
            var httpRequestMsg = new HttpRequestMessage () {
                Method = HttpMethod.Get,
                RequestUri = new Uri (httpClient.BaseAddress, $"tenant/exists?domain={domain}")
            };
            apiResponse = await SendAsync (httpRequestMsg);
            if (apiResponse.IsSuccessStatusCode) {
                return true;
            } else {
                return false;
            }

        }

        public async Task<T> GetTenantAsync<T> (string domain) {
            var apiResponse = new InternalApiResponse<T> ();
            var httpRequestMsg = new HttpRequestMessage () {
                Method = HttpMethod.Get,
                RequestUri = new Uri (httpClient.BaseAddress, $"/tenant?domain={domain}")
            };
            apiResponse = await SendAsync<T> (httpRequestMsg);
            if (apiResponse.IsSuccessStatusCode) {
                return apiResponse.ResponseObject;
            } else {
                throw new Exception (String.Join (Environment.NewLine, apiResponse.Errors));
            }
        }
    }
}