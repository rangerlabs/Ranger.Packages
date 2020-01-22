using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ranger.InternalHttpClient
{
    public class OperationsClient : ApiClientBase, IOperationsClient
    {
        private readonly ILogger<OperationsClient> logger;

        public OperationsClient(string uri, ILogger<OperationsClient> logger) : base(uri, logger, "operationsApi")
        {
            this.logger = logger;
        }

        public async Task<T> GetOperationStateById<T>(string domain, string id)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"{nameof(id)} cannot be null or whitespace.");
            }

            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"/{domain}/operations/{id}")
                };
            });
            var apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            return apiResponse.IsSuccessStatusCode ? apiResponse.ResponseObject : throw new HttpClientException<T>(apiResponse);
        }
    }
}