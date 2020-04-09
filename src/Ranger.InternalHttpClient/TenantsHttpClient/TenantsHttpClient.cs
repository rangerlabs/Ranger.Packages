using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{
    public class TenantsHttpClient : ApiClientBase
    {
        public TenantsHttpClient(HttpClient httpClient, ILogger<TenantsHttpClient> logger) : base(httpClient, logger)
        { }

        public async Task<ApiResponse<T>> ExistsAsync<T>(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            return await SendAsync<T>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"tenant/{domain}/exists")
            });
        }

        public async Task<ApiResponse<T>> EnabledAsync<T>(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            return await SendAsync<T>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"tenant/{domain}/enabled")
            });
        }

        public async Task<ApiResponse<T>> GetPrimaryOwnerTransferByDomain<T>(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            return await SendAsync<T>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/tenant/{domain}/primary-owner-transfer"),
            });
        }

        public async Task<ApiResponse<T>> GetTenantByDatabaseUsernameAsync<T>(string databaseUsername)
        {
            if (String.IsNullOrWhiteSpace(databaseUsername))
            {
                throw new ArgumentException($"{nameof(databaseUsername)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/tenant?databaseUsername={databaseUsername}"),
            });
        }

        public async Task<ApiResponse<T>> GetTenantAsync<T>(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/tenant/{domain}"),
            });
        }

        public async Task<ApiResponse<T>> ConfirmTenantAsync<T>(string domain, string jsonContent)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (String.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/tenant/{domain}/confirm"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });
        }
    }
}