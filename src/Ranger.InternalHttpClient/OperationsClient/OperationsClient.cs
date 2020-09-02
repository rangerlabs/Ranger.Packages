using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{

    public class OperationsClient : ApiClientBase, IOperationsClient
    {
        public OperationsClient(HttpClient httpClient, HttpClientOptions<OperationsClient> clientOptions, ILogger<OperationsClient> logger) : base(httpClient, clientOptions, logger)
        { }

        public async Task<RangerApiResponse<T>> GetOperationStateById<T>(string domain, Guid id, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/{domain}/operations/{id}")
            }, cancellationToken);
        }
    }
}