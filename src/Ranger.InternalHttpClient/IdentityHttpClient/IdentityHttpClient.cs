using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{
    public class IdentityHttpClient : ApiClientBase
    {
        public IdentityHttpClient(HttpClient httpClient, HttpClientOptions<IdentityHttpClient> clientOptions, ILogger<IdentityHttpClient> logger) : base(httpClient, clientOptions, logger)
        { }

        ///<summary>
        /// Produces 200, 400, 404
        ///</summary>
        public async Task<RangerApiResponse> DeleteAccountAsync(string tenantId, string email, string jsonContent)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace");
            }

            return await SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/users/{tenantId}/{email}/account"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }

        ///<summary>
        /// Produces 200, 403, 404
        ///</summary>
        public async Task<RangerApiResponse> DeleteUserAsync(string tenantId, string email, string jsonContent)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace");
            }

            return await SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/users/{tenantId}/{email}"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }

        ///<summary>
        /// Produces 200, 400, 404
        ///</summary>
        public async Task<RangerApiResponse> UpdateUserOrAccountAsync(string tenantId, string email, string jsonContent)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace");
            }

            return await SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/users/{tenantId}/{email}"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }

        ///<summary>
        /// Produces 200, 404
        ///</summary>
        public async Task<RangerApiResponse<T>> GetUserAsync<T>(string tenantId, string email)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/users/{tenantId}/{email}"),
            });
        }

        ///<summary>
        /// Produces 200
        ///</summary>
        public async Task<RangerApiResponse<T>> GetAllUsersAsync<T>(string tenantId)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/users/{tenantId}"),
            });
        }

        ///<summary>
        /// Produces 200, 404
        ///</summary>
        public async Task<RangerApiResponse<string>> GetUserRoleAsync(string tenantId, string email)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace");
            }

            return await SendAsync<string>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/users/{tenantId}/{email}/role"),
            });
        }

        ///<summary>
        /// Produces 200, 304, 404
        ///</summary>
        public async Task<RangerApiResponse> RequestPasswordReset(string tenantId, string email, string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace");
            }

            return await SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/users/{tenantId}/{email}/password-reset"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }

        ///<summary>
        /// Produces 200, 403, 404, 409
        ///</summary>
        public async Task<RangerApiResponse> RequestEmailChange(string tenantId, string email, string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace");
            }

            return await SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/users/{tenantId}/{email}/email-change"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }

        ///<summary>
        /// Produces 200, 400, 404
        ///</summary>
        public async Task<RangerApiResponse> ConfirmUserAsync(string tenantId, string email, string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace");
            }

            return await SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/users/{tenantId}/{email}/confirm"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }

        ///<summary>
        /// Produces 200, 400, 404
        ///</summary>
        public async Task<RangerApiResponse> UserConfirmPasswordResetAsync(string tenantId, string email, string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace");
            }

            return await SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/users/{tenantId}/{email}/password-reset"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }

        ///<summary>
        /// Produces 200, 404
        ///</summary>
        public async Task<RangerApiResponse> UserConfirmEmailChangeAsync(string tenantId, string userId, string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} cannot be null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException($"{nameof(userId)} cannot be null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace");
            }

            return await SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(HttpClient.BaseAddress, $"/users/{tenantId}/{userId}/email-change"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }
    }
}