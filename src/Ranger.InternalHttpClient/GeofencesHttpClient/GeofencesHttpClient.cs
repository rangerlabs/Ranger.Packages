using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{
    public class GeofencesHttpClient : ApiClientBase
    {
        public GeofencesHttpClient(HttpClient httpClient, HttpClientOptions<GeofencesHttpClient> clientOptions, ILogger<GeofencesHttpClient> logger) : base(httpClient, clientOptions, logger)
        { }

        ///<summary>
        /// Produces 200
        ///</summary>
        public async Task<RangerApiResponse<T>> GetAllGeofencesByProjectId<T>(string tenantId, Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/{tenantId}/geofences/{projectId}")
            });
        }
    }
}