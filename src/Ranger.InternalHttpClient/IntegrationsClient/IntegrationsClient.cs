using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{
    public class IntegrationsClient : ApiClientBase
    {
        public IntegrationsClient(HttpClient httpClient, ILogger<IntegrationsClient> logger) : base(httpClient, logger)
        { }

        public async Task<ApiResponse<T>> GetAllIntegrationsByProjectId<T>(string domain, Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/{domain}/integrations?projectId={projectId}")
            });
        }
    }
}