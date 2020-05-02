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
        public SubscriptionsHttpClient(HttpClient httpClient, HttpClientOptions<SubscriptionsHttpClient> clientOptions, ILogger<SubscriptionsHttpClient> logger) : base(httpClient, clientOptions, logger)
        { }

        ///<summary>
        /// Produces 200, 404
        ///</summary>
        public async Task<RangerApiResponse<T>> GenerateCheckoutExistingUrl<T>(string tenantId, string planId)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            if (String.IsNullOrWhiteSpace(planId))
            {
                throw new ArgumentException($"{nameof(planId)} cannot be null or whitespace");
            }
            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/subscriptions/{tenantId}/{planId}/checkout-existing-hosted-page-url")
            });
        }
        ///<summary>
        /// Produces 200, 404
        ///</summary>
        public async Task<RangerApiResponse<T>> GetLimitDetails<T>(string tenantId)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/subscriptions/{tenantId}/limit-details")
            });
        }
        ///<summary>
        /// Produces 200, 404
        ///</summary>
        public async Task<RangerApiResponse<string>> GetSubscriptionPlanId(string tenantId)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            return await SendAsync<string>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/subscriptions/{tenantId}/plan-id")
            });
        }
    }
}