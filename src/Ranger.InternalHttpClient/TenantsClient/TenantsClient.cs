using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{
    public class TenantsClient : ApiClientBase, ITenantsClient
    {
        private readonly ILogger<TenantsClient> logger;

        public TenantsClient(string uri, ILogger<TenantsClient> logger) : base(uri, logger, "tenantsApi")
        {
            this.logger = logger;
        }

        public async Task<bool> ExistsAsync(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"tenant/{domain}/exists")
                };
            });
            var apiResponse = await SendAsync(httpRequestMessageFactory);
            return apiResponse.IsSuccessStatusCode ? true : false;
        }

        public async Task<T> EnabledAsync<T>(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"tenant/{domain}/enabled")
                };
            });
            var apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return apiResponse.ResponseObject;
            }
            throw new HttpClientException<T>(apiResponse);
        }

        public async Task<T> GetTenantByDatabaseUsernameAsync<T>(string databaseUsername)
        {
            if (String.IsNullOrWhiteSpace(databaseUsername))
            {
                throw new ArgumentException($"{nameof(databaseUsername)} cannot be null or whitespace.");
            }

            var apiResponse = new InternalApiResponse<T>();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"/tenant?databaseUsername={databaseUsername}"),
                };
            });
            apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return apiResponse.ResponseObject;
            }
            throw new HttpClientException<T>(apiResponse);
        }

        public async Task<T> GetTenantAsync<T>(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            var apiResponse = new InternalApiResponse<T>();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"/tenant/{domain}"),
                };
            });
            apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return apiResponse.ResponseObject;
            }
            throw new HttpClientException<T>(apiResponse);
        }

        public async Task<bool> ConfirmTenantAsync(string domain, string jsonContent)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (String.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace.");
            }

            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(httpClient.BaseAddress, $"/tenant/{domain}/confirm"),
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };
            });
            var apiResponse = await SendAsync(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return true;
            }
            if ((int)apiResponse.StatusCode == StatusCodes.Status409Conflict)
            {
                return false;
            }
            throw new HttpClientException(apiResponse);
        }
    }
}