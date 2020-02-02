using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{
    public class IntegrationsClient : ApiClientBase, IIntegrationsClient
    {
        private readonly ILogger<IntegrationsClient> logger;

        public IntegrationsClient(string uri, ILogger<IntegrationsClient> logger) : base(uri, logger, "integrationsApi")
        {
            this.logger = logger;
        }

        public async Task<T> GetAllIntegrationsByProjectId<T>(string domain, Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"/{domain}/integrations?projectId={projectId}")
                };
            });
            var apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            return apiResponse.IsSuccessStatusCode ? apiResponse.ResponseObject : throw new HttpClientException<T>(apiResponse);
        }
    }
}