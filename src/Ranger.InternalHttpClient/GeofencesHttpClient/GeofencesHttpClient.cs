using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{

    public class GeofencesHttpClient : ApiClientBase, IGeofencesHttpClient
    {
        public GeofencesHttpClient(HttpClient httpClient, HttpClientOptions<GeofencesHttpClient> clientOptions, ILogger<GeofencesHttpClient> logger) : base(httpClient, clientOptions, logger)
        { }

        ///<summary>
        /// Produces 200
        ///</summary>
        public async Task<RangerApiResponse<T>> GetAllGeofencesByProjectId<T>(string tenantId, Guid projectId, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/geofences/{tenantId}/{projectId}")
            }, cancellationToken);
        }

        ///<summary>
        /// Produces 200
        ///</summary>
        public async Task<RangerApiResponse<long>> GetAllActiveGeofencesCount(string tenantId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await SendAsync<long>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/geofences/{tenantId}/count")
            }, cancellationToken);
        }
    }
}