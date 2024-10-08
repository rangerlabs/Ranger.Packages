using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{

    public class TenantsHttpClient : ApiClientBase, ITenantsHttpClient
    {
        public TenantsHttpClient(HttpClient httpClient, HttpClientOptions<TenantsHttpClient> clientOptions, ILogger<TenantsHttpClient> logger) : base(httpClient, clientOptions, logger)
        { }

        /// <summary>
        /// Produces 200
        /// </summary>
        public async Task<RangerApiResponse<bool>> DoesExistAsync(string domain, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace");
            }
            return await SendAsync<bool>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"tenants/{domain}/exists")
            }, cancellationToken);
        }

        /// <summary>
        /// Produces 200, 404
        /// </summary>
        public async Task<RangerApiResponse<bool>> IsConfirmedAsync(string domain, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace");
            }
            return await SendAsync<bool>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"tenants/{domain}/confirmed")
            }, cancellationToken);
        }

        /// <summary>
        /// Produces 200, 404
        /// </summary>
        public async Task<RangerApiResponse<T>> GetPrimaryOwnerTransferByDomain<T>(string domain, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace");
            }
            return await SendAsync<T>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/tenants/{domain}/primary-owner-transfer"),
            }, cancellationToken);
        }

        /// <summary>
        /// Produces 200, 404
        /// </summary>
        public async Task<RangerApiResponse<T>> GetAllTenantsAsync<T>(CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            return await SendAsync<T>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/tenants"),
            }, cancellationToken);
        }

        /// <summary>
        /// Produces 200, 404
        /// </summary>
        public async Task<RangerApiResponse<T>> GetTenantByIdAsync<T>(string tenantId, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            return await SendAsync<T>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/tenants?tenantId={tenantId}"),
            }, cancellationToken);
        }

        /// <summary>
        /// Produces 200, 404
        /// </summary>
        public async Task<RangerApiResponse<T>> GetTenantByDomainAsync<T>(string domain, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace");
            }
            return await SendAsync<T>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/tenants?domain={domain}"),
            }, cancellationToken);
        }

        /// <summary>
        /// Produces 200, 400, 404
        /// </summary>
        public async Task<RangerApiResponse> ConfirmTenantAsync(string domain, string jsonContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace");
            }
            if (String.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace");
            }
            return await SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/tenants/{domain}/confirm"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            }, cancellationToken);
        }
    }
}