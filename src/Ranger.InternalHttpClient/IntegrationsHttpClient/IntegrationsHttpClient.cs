using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{
    public class IntegrationsHttpClient : ApiClientBase
    {
        public IntegrationsHttpClient(HttpClient httpClient, HttpClientOptions<IntegrationsHttpClient> clientOptions, ILogger<IntegrationsHttpClient> logger) : base(httpClient, clientOptions, logger)
        { }

        ///<summary>
        /// Produces 200
        ///</summary>
        public async Task<RangerApiResponse<T>> GetAllIntegrationsByProjectId<T>(string tenantId, Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/integrations/{tenantId}/{projectId}")
            });
        }

        ///<summary>
        /// Produces 200
        ///</summary>
        public async Task<RangerApiResponse<int>> GetAllActiveIntegrationsCount(string tenantId)
        {
            return await SendAsync<int>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/integrations/{tenantId}/count")
            });
        }
    }
}