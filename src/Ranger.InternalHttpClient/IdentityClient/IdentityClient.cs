using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{
    public class IdentityClient : ApiClientBase
    {
        public IdentityClient(HttpClient httpClient, ILogger<IdentityClient> logger) : base(httpClient, logger)
        { }

        public async Task<ApiResponse<T>> DeleteAccountAsync<T>(string domain, string email, string jsonContent)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/users/{email}/account"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }

        public async Task<ApiResponse<T>> DeleteUserAsync<T>(string domain, string email, string jsonContent)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/users/{email}"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }

        public async Task<ApiResponse<T>> UpdateUserAsync<T>(string domain, string username, string jsonContent)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException($"{nameof(username)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/users/{username}"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }

        public async Task<ApiResponse<T>> GetUserAsync<T>(string domain, string username)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException($"{nameof(username)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/users/{username}"),
            });
        }

        public async Task<ApiResponse<T>> GetAllUsersAsync<T>(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/users"),
            });
        }

        public async Task<ApiResponse<T>> GetUserRoleAsync<T>(string domain, string email)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/users/{email}/role"),
            });
        }

        public async Task<ApiResponse<bool>> RequestPasswordReset(string domain, string email, string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace.");
            }

            return await SendAsync<bool>(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/users/{email}/password-reset"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }

        public async Task<ApiResponse<bool>> RequestEmailChange(string domain, string email, string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace.");
            }

            return await SendAsync<bool>(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/users/{email}/email-change"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }

        public async Task<ApiResponse<bool>> ConfirmUserAsync(string domain, string userId, string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException($"{nameof(userId)} cannot be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace.");
            }

            return await SendAsync<bool>(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/users/{userId}/confirm"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }

        public async Task<ApiResponse<bool>> UserConfirmPasswordResetAsync(string domain, string userId, string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException($"{nameof(userId)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace.");
            }

            return await SendAsync<bool>(new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/users/{userId}/password-reset"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }

        public async Task<ApiResponse<T>> UserConfirmEmailChangeAsync<T>(string domain, string userId, string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException($"{nameof(userId)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException($"{nameof(jsonContent)} cannot be null or whitespace.");
            }

            return await SendAsync<T>(new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(HttpClient.BaseAddress, $"{domain}/users/{userId}/email-change"),
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });
        }
    }
}