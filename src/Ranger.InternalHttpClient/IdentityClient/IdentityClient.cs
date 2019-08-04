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
    public class IdentityClient : ApiClientBase<IdentityClient>, IIdentityClient {
        public IdentityClient (string uri, ILogger<IdentityClient> logger) : base (uri, logger) { }

        public async Task<T> GetUserAsync<T> (string domain, string username) {
            var apiResponse = new InternalApiResponse<T> ();
            var httpRequestMsg = new HttpRequestMessage () {
                Method = HttpMethod.Get,
                RequestUri = new Uri (httpClient.BaseAddress, $"user/{username}"),
                Headers = { { "X-Tenant-Domain", domain },
                }
            };
            apiResponse = await SendAsync<T> (httpRequestMsg);
            if (apiResponse.IsSuccessStatusCode) {
                return apiResponse.ResponseObject;
            } else {
                throw new Exception (String.Join (Environment.NewLine, apiResponse.Errors));
            }
        }

        public async Task<T> GetAllUsersAsync<T> (string domain) {
            var apiResponse = new InternalApiResponse<T> ();
            var httpRequestMsg = new HttpRequestMessage () {
                Method = HttpMethod.Get,
                RequestUri = new Uri (httpClient.BaseAddress, $"user/all"),
                Headers = { { "X-Tenant-Domain", domain },
                }
            };
            apiResponse = await SendAsync<T> (httpRequestMsg);
            if (apiResponse.IsSuccessStatusCode) {
                return apiResponse.ResponseObject;
            } else {
                throw new Exception (String.Join (Environment.NewLine, apiResponse.Errors));
            }
        }

        public async Task<T> GetRoleAsync<T> (string name) {
            var apiResponse = new InternalApiResponse<T> ();
            var httpRequestMsg = new HttpRequestMessage () {
                Method = HttpMethod.Get,
                RequestUri = new Uri ($"role/{name}")
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