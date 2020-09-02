using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;

namespace Ranger.InternalHttpClient
{

    public class ProjectsHttpClient : ApiClientBase, IProjectsHttpClient
    {
        public ProjectsHttpClient(HttpClient httpClient, HttpClientOptions<ProjectsHttpClient> clientOptions, ILogger<ProjectsHttpClient> logger) : base(httpClient, clientOptions, logger)
        { }

        ///<summary>
        /// Produces 200, 400, 404
        ///</summary>
        public async Task<RangerApiResponse<string>> GetTenantIdByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException($"{nameof(apiKey)} cannot be null or whitespace");
            }

            return await SendAsync<string>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/projects/{apiKey}/tenant-id"),
            }, cancellationToken);
        }

        ///<summary>
        /// Produces 200, 400, 404
        ///</summary>
        public async Task<RangerApiResponse<T>> GetAllProjects<T>(string tenantId, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/projects/{tenantId}"),
            }, cancellationToken);
        }

        ///<summary>
        /// Produces 200, 400, 404
        ///</summary>
        public async Task<RangerApiResponse<T>> GetProjectByNameAsync<T>(string tenantId, string projectName, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(projectName))
            {
                throw new ArgumentException($"{nameof(projectName)} cannot be null or whitespace");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/projects/{tenantId}?projectName={projectName}"),
            }, cancellationToken);
        }

        ///<summary>
        /// Produces 200, 400, 404
        ///</summary>
        public async Task<RangerApiResponse<T>> GetProjectByApiKeyAsync<T>(string tenantId, string apiKey, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException($"{nameof(apiKey)} cannot be null or whitespace");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/projects/{tenantId}?apiKey={apiKey}"),
            }, cancellationToken);
        }

        ///<summary>
        /// Produces 200, 400, 404
        ///</summary>
        public async Task<RangerApiResponse<T>> GetAllProjectsForUserAsync<T>(string tenantId, string email, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            if (String.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/projects/{tenantId}?email={email}"),
            }, cancellationToken);
        }

        ///<summary>
        /// Produces 200, 400, 409
        ///</summary>
        public async Task<RangerApiResponse<T>> PutProjectAsync<T>(string tenantId, Guid projectId, string jsonContent, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            if (String.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace");
            }
            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/projects/{tenantId}/{projectId}"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            }, cancellationToken);
        }

        ///<summary>
        /// Produces 200, 400, 409
        ///</summary>
        public async Task<RangerApiResponse<T>> ApiKeyResetAsync<T>(string tenantId, Guid projectId, ApiKeyPurposeEnum purpose, string jsonContent, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            if (String.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace");
            }
            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/projects/{tenantId}/{projectId}/{purpose}/reset"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            }, cancellationToken);
        }

        ///<summary>
        /// Produces 200, 400, 409
        ///</summary>
        public async Task<RangerApiResponse> SoftDeleteProjectAsync(string tenantId, Guid projectId, string userEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }

            return await SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/projects/{tenantId}/{projectId}"),
                Content = new StringContent(JsonConvert.SerializeObject(new { UserEmail = userEmail }), Encoding.UTF8, "application/json")
            }, cancellationToken);
        }

        ///<summary>
        /// Produces 201
        ///</summary>
        public async Task<RangerApiResponse<T>> PostProjectAsync<T>(string tenantId, string jsonContent, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            if (String.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/projects/{tenantId}"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            }, cancellationToken);
        }
    }
}