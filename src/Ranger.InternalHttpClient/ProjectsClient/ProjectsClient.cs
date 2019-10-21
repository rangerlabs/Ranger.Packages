using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{
    public class ProjectsClient : ApiClientBase, IProjectsClient
    {
        private readonly ILogger<ProjectsClient> logger;

        public ProjectsClient(string uri, ILogger<ProjectsClient> logger) : base(uri, logger, "projectsApi")
        {
            this.logger = logger;
        }

        public async Task<T> GetDatabaseUsernameByApiKeyAsync<T>(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException($"{nameof(apiKey)} cannot be null or whitespace.");
            }

            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"/project/{apiKey}/databaseusername"),
                };
            });

            var apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return apiResponse.ResponseObject;
            }
            else
            {
                throw new HttpClientException<T>(apiResponse);
            }
        }

        public async Task<T> GetProjectByApiKeyAsync<T>(string domain, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException($"{nameof(apiKey)} cannot be null or whitespace.");
            }

            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"/{domain}/project?apiKey={apiKey}"),
                };
            });

            var apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return apiResponse.ResponseObject;
            }
            else
            {
                throw new HttpClientException<T>(apiResponse);
            }
        }

        public async Task<T> GetAllProjectsAsync<T>(string domain)
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
                    RequestUri = new Uri(httpClient.BaseAddress, $"/{domain}/project/all"),
                };
            });

            var apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return apiResponse.ResponseObject;
            }
            else
            {
                throw new HttpClientException<T>(apiResponse);
            }
        }

        public async Task<T> PutProjectAsync<T>(HttpMethod method, string domain, string projectId, string jsonContent)
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
                    Method = method,
                    RequestUri = new Uri(httpClient.BaseAddress, $"/{domain}/project/{projectId}"),
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };
            });
            var apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return apiResponse.ResponseObject;
            }
            throw new HttpClientException<T>(apiResponse);
        }

        public async Task<T> PostProjectAsync<T>(HttpMethod method, string domain, string jsonContent)
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
                    Method = method,
                    RequestUri = new Uri(httpClient.BaseAddress, $"/{domain}/project"),
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };
            });
            var apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return apiResponse.ResponseObject;
            }
            throw new HttpClientException<T>(apiResponse);
        }
    }
}