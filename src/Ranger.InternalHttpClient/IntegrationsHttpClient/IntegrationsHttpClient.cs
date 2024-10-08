using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{

    public class IntegrationsHttpClient : ApiClientBase, IIntegrationsHttpClient
    {
        public IntegrationsHttpClient(HttpClient httpClient, HttpClientOptions<IntegrationsHttpClient> clientOptions, ILogger<IntegrationsHttpClient> logger) : base(httpClient, clientOptions, logger)
        { }

        ///<summary>
        /// Produces 200
        ///</summary>
        public async Task<RangerApiResponse<T>> GetAllIntegrationsByProjectId<T>(string tenantId, Guid projectId, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/integrations/{tenantId}/{projectId}")
            }, cancellationToken);
        }

        ///<summary>
        /// Produces 200
        ///</summary>
        public async Task<RangerApiResponse<int>> GetAllActiveIntegrationsCount(string tenantId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await SendAsync<int>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/integrations/{tenantId}/count")
            }, cancellationToken);
        }
    }
}