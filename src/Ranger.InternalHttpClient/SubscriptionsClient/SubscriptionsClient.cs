using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ranger.InternalHttpClient
{
    public class SubscriptionsClient : ApiClientBase, ISubscriptionsClient
    {
        private readonly ILogger<SubscriptionsClient> logger;

        public SubscriptionsClient(string uri, ILogger<SubscriptionsClient> logger) : base(uri, logger, "subscriptionsApi")
        {
            this.logger = logger;
        }

        public async Task<T> GenerateCheckoutExistingUrl<T>(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"{domain}/subscriptions/checkout-existing-hosted-page-url")
                };
            });
            var apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return apiResponse.ResponseObject;
            }
            throw new HttpClientException<T>(apiResponse);
        }
    }
}