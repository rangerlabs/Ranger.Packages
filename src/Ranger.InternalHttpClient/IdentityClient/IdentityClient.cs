using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{
    public class IdentityClient : ApiClientBase, IIdentityClient
    {
        private readonly ILogger<IdentityClient> logger;

        public IdentityClient(string uri, ILogger<IdentityClient> logger) : base(uri, logger, IdentityServerConstants.LocalApi.ScopeName)
        {
            this.logger = logger;
        }

        public async Task<T> GetUserAsync<T>(string domain, string username)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            var apiResponse = new InternalApiResponse<T>();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"user/{username}"),
                    Headers = { { "x-ranger-domain", domain },
                }
                };
            });
            apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            return apiResponse.IsSuccessStatusCode ? apiResponse.ResponseObject : throw new HttpClientException<T>(apiResponse);
        }

        public async Task<T> GetAllUsersAsync<T>(string domain)
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
                    RequestUri = new Uri(httpClient.BaseAddress, $"user/all"),
                    Headers = { { "x-ranger-domain", domain },
                }
                };
            });
            apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            return apiResponse.IsSuccessStatusCode ? apiResponse.ResponseObject : throw new HttpClientException<T>(apiResponse);
        }

        public async Task<T> GetRoleAsync<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{nameof(name)} cannot be null or whitespace.");
            }

            var apiResponse = new InternalApiResponse<T>();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"role/{name}")
                };
            });
            apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            return apiResponse.IsSuccessStatusCode ? apiResponse.ResponseObject : throw new HttpClientException<T>(apiResponse);
        }

        public async Task<bool> ConfirmUserAsync(string domain, string registrationKey)
        {
            if (string.IsNullOrWhiteSpace(registrationKey))
            {
                throw new ArgumentException($"{nameof(registrationKey)} cannot be null or whitespace.");
            }

            var apiResponse = new InternalApiResponse();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"user/confirm?registrationKey={registrationKey}"),
                    Headers = { { "x-ranger-domain", domain } }
                };
            });
            apiResponse = await SendAsync(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return true;
            }
            if ((int)apiResponse.StatusCode == StatusCodes.Status304NotModified)
            {
                return false;
            }
            throw new HttpClientException(apiResponse);
        }
    }
}