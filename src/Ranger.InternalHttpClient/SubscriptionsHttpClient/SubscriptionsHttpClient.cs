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
    public class SubscriptionsHttpClient : ApiClientBase
    {
        public SubscriptionsHttpClient(HttpClient httpClient, ILogger<SubscriptionsHttpClient> logger) : base(httpClient, logger)
        { }

        ///<summary>
        /// Produces 200
        ///<summary>
        public async Task<ApiResponse<string>> GenerateCheckoutExistingUrl(string tenantId, string planId)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace.");
            }
            if (String.IsNullOrWhiteSpace(planId))
            {
                throw new ArgumentException($"{nameof(planId)} cannot be null or whitespace.");
            }
            return await SendAsync<string>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/subscriptions/{tenantId}/{planId}/checkout-existing-hosted-page-url")
            });
        }

        public async Task<ApiResponse<T>> GetLimitDetails<T>(string tenantId)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace.");
            }
            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/subscriptions/{tenantId}/limit-details")
            });
        }

        public async Task<ApiResponse<string>> GetSubscriptionPlanId(string tenantId)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace.");
            }
            return await SendAsync<string>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/subscriptions/{tenantId}/plan-id")
            });
        }

        public async Task<ApiResponse<int>> IncrementResource(string tenantId, ResourceEnum resource)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace.");
            }
            return await SendAsync<int>(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/subscriptions/{tenantId}/resources/increment"),
                Content = new StringContent(JsonConvert.SerializeObject(new { Resource = resource }), Encoding.UTF8, "application/json")
            });
        }

        public async Task<ApiResponse<int>> DecrementResource(string tenantId, ResourceEnum resource)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace.");
            }
            return await SendAsync<int>(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/subscriptions/{tenantId}/resources/decrement"),
                Content = new StringContent(JsonConvert.SerializeObject(new { Resource = resource }), Encoding.UTF8, "application/json")
            });
        }
    }
}