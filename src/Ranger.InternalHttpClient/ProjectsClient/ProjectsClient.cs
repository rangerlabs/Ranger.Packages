using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ranger.InternalHttpClient
{
    public class ProjectsClient : ApiClientBase
    {
        public ProjectsClient(HttpClient httpClient, ILogger<ProjectsClient> logger) : base(httpClient, logger)
        { }

        public async Task<ApiResponse<IEnumerable<string>>> GetProjectIdsForUser(string domain, string email)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace.");
            }

            return await SendAsync<IEnumerable<string>>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/{domain}/project/authorized/{email}")
            });
        }

        public async Task<ApiResponse<T>> GetDatabaseUsernameByApiKeyAsync<T>(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException($"{nameof(apiKey)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/project/{apiKey}/databaseusername"),
            });
        }

        public async Task<ApiResponse<T>> GetProjectByApiKeyAsync<T>(string domain, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException($"{nameof(apiKey)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/{domain}/project?apiKey={apiKey}"),
            });
        }

        public async Task<ApiResponse<T>> GetAllProjectsForUserAsync<T>(string domain, string email)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (String.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/{domain}/project/{email}"),
            });
        }

        public async Task<ApiResponse<T>> PutProjectAsync<T>(string domain, Guid projectId, string jsonContent)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (String.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace.");
            }
            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/{domain}/project/{projectId}"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });
        }

        public async Task<ApiResponse<T>> ApiKeyResetAsync<T>(string domain, Guid projectId, string environment, string jsonContent)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (String.IsNullOrWhiteSpace(environment))
            {
                throw new ArgumentException($"{nameof(environment)} cannot be null or whitespace.");
            }
            if (String.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace.");
            }
            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/{domain}/project/{projectId}/{environment}/reset"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });
        }

        public async Task<ApiResponse<T>> SoftDeleteProjectAsync<T>(string domain, Guid projectId, string userEmail)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/{domain}/project/{projectId}"),
                Content = new StringContent(JsonConvert.SerializeObject(new { UserEmail = userEmail }), Encoding.UTF8, "application/json")
            });
        }

        public async Task<ApiResponse<T>> PostProjectAsync<T>(string domain, string jsonContent)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (String.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/{domain}/project"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });
        }
    }
}