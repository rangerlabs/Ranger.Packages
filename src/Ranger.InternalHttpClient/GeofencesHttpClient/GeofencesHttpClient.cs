using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;

namespace Ranger.InternalHttpClient
{

    public class GeofencesHttpClient : ApiClientBase, IGeofencesHttpClient
    {
        public GeofencesHttpClient(HttpClient httpClient, HttpClientOptions<GeofencesHttpClient> clientOptions, ILogger<GeofencesHttpClient> logger) : base(httpClient, clientOptions, logger)
        { }

        ///<summary>
        /// Produces 200
        ///</summary>
        public async Task<RangerApiResponse<T>> GetGeofenceByExternalId<T>(string tenantId, Guid projectId, string externalId, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new ArgumentException($"'{nameof(externalId)}' cannot be null or whitespace", nameof(externalId));
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/geofences/{tenantId}/{projectId}?externalId={externalId}")
            }, cancellationToken);
        }

        ///<summary>
        /// Produces 200
        ///</summary>
        public async Task<RangerApiResponse<T>> GetGeofencesByBounds<T>(string tenantId, Guid projectId, string orderBy, string sortOrder, IEnumerable<LngLat> bounds, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }

            var serializedBounds = bounds.Select(f => JsonConvert.SerializeObject(f));
            var boundsQuery = String.Join(';', serializedBounds);

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/geofences/{tenantId}/{projectId}?orderBy={orderBy}&sortOrder={sortOrder}&bounds={boundsQuery}")
            }, cancellationToken);
        }

        ///<summary>
        /// Produces 200
        ///</summary>
        public async Task<RangerApiResponse<T>> GetGeofencesByProjectId<T>(string tenantId, Guid projectId, string orderBy, string sortOrder, int page, int pageCount, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/geofences/{tenantId}/{projectId}?orderBy={orderBy}&sortOrder={sortOrder}&page={page}&pageCount={pageCount}")
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