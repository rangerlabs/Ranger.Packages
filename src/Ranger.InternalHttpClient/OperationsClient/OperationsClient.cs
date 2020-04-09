using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{
    public class OperationsClient : ApiClientBase
    {
        public OperationsClient(HttpClient httpClient, ILogger<OperationsClient> logger) : base(httpClient, logger)
        { }

        public async Task<ApiResponse<T>> GetOperationStateById<T>(string domain, Guid id)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/{domain}/operations/{id}")
            });
        }
    }
}