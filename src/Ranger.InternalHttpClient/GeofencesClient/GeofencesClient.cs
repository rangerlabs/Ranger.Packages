using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ranger.InternalHttpClient
{
    public class GeofencesClient : ApiClientBase, IGeofencesClient
    {
        private readonly ILogger<GeofencesClient> logger;

        public GeofencesClient(string uri, ILogger<GeofencesClient> logger) : base(uri, logger, "geofencesApi")
        {
            this.logger = logger;
        }

        public async Task<T> GetAllGeofencesByProjectId<T>(string domain, string projectId)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(projectId))
            {
                throw new ArgumentException($"{nameof(projectId)} cannot be null or whitespace.");
            }

            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"/{domain}/geofences?projectId={projectId}")
                };
            });
            var apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            return apiResponse.IsSuccessStatusCode ? apiResponse.ResponseObject : throw new HttpClientException<T>(apiResponse);
        }
    }
}