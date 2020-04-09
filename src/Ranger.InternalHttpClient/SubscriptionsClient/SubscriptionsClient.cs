using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;

namespace Ranger.InternalHttpClient
{
    public class SubscriptionsClient : ApiClientBase
    {
        public SubscriptionsClient(HttpClient httpClient, ILogger<SubscriptionsClient> logger) : base(httpClient, logger)
        { }

        public async Task<ApiResponse<T>> GenerateCheckoutExistingUrl<T>(string domain, string planId)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (String.IsNullOrWhiteSpace(planId))
            {
                throw new ArgumentException($"{nameof(planId)} cannot be null or whitespace.");
            }
            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/subscriptions/checkout-existing-hosted-page-url?planId={planId}")
            });

        }

        public async Task<ApiResponse<T>> GetLimitDetails<T>(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/subscriptions/limit-details")
            });
        }

        public async Task<ApiResponse<T>> GetSubscriptionPlanId<T>(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/subscriptions/plan-id")
            });
        }

        public async Task<ApiResponse<T>> IncrementResource<T>(string domain, ResourceEnum resource)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/subscriptions/resources/increment"),
                Content = new StringContent(JsonConvert.SerializeObject(new { Resource = resource }), Encoding.UTF8, "application/json")
            });
        }

        public async Task<ApiResponse<T>> DecrementResource<T>(string domain, ResourceEnum resource)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/subscriptions/resources/decrement"),
                Content = new StringContent(JsonConvert.SerializeObject(new { Resource = resource }), Encoding.UTF8, "application/json")
            });
        }
    }
}