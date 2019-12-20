using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ranger.InternalHttpClient
{
    public class IdentityClient : ApiClientBase, IIdentityClient
    {
        private readonly ILogger<IdentityClient> logger;

        public IdentityClient(string uri, ILogger<IdentityClient> logger) : base(uri, logger, IdentityServerConstants.LocalApi.ScopeName)
        {
            this.logger = logger;
        }

        public async Task DeleteAccountAsync(string domain, string email, string jsonContent)
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


            var apiResponse = new InternalApiResponse();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(httpClient.BaseAddress, $"user/{email}/account"),
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
                    Headers = { { "x-ranger-domain", domain },
                }
                };
            });
            apiResponse = await SendAsync(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return;
            }
            throw new HttpClientException(apiResponse);
        }

        public async Task DeleteUserAsync(string domain, string email, string jsonContent)
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

            var apiResponse = new InternalApiResponse();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(httpClient.BaseAddress, $"user/{email}"),
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
                    Headers = { { "x-ranger-domain", domain },
                }
                };
            });
            apiResponse = await SendAsync(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return;
            }
            throw new HttpClientException(apiResponse);
        }

        public async Task UpdateUserAsync(string domain, string username, string jsonContent)
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

            var apiResponse = new InternalApiResponse();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(httpClient.BaseAddress, $"user/{username}"),
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
                    Headers = { { "x-ranger-domain", domain },
                }
                };
            });
            apiResponse = await SendAsync(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return;
            }
            throw new HttpClientException(apiResponse);
        }

        public async Task<T> GetUserAsync<T>(string domain, string username)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException($"{nameof(username)} cannot be null or whitespace.");
            }

            var apiResponse = new InternalApiResponse<T>();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"user/{username}"),
                    Headers = { { "x-ranger-domain", domain },
                }
                };
            });
            apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            return apiResponse.IsSuccessStatusCode ? apiResponse.ResponseObject : throw new HttpClientException<T>(apiResponse);
        }

        public async Task<T> GetAllUsersAsync<T>(string domain)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }

            var apiResponse = new InternalApiResponse<T>();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"user/all"),
                    Headers = { { "x-ranger-domain", domain },
                }
                };
            });
            apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            return apiResponse.IsSuccessStatusCode ? apiResponse.ResponseObject : throw new HttpClientException<T>(apiResponse);
        }

        public async Task<T> GetRoleAsync<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{nameof(name)} cannot be null or whitespace.");
            }

            var apiResponse = new InternalApiResponse<T>();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"role/{name}")
                };
            });
            apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            return apiResponse.IsSuccessStatusCode ? apiResponse.ResponseObject : throw new HttpClientException<T>(apiResponse);
        }

        public async Task<T> GetUserRoleAsync<T>(string domain, string email)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} cannot be null or whitespace.");
            }

            var apiResponse = new InternalApiResponse<T>();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(httpClient.BaseAddress, $"user/{email}/role"),
                    Headers = { { "x-ranger-domain", domain } }
                };
            });
            apiResponse = await SendAsync<T>(httpRequestMessageFactory);
            return apiResponse.IsSuccessStatusCode ? apiResponse.ResponseObject : throw new HttpClientException<T>(apiResponse);
        }

        public async Task<bool> RequestPasswordReset(string domain, string email, string jsonContent)
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

            var apiResponse = new InternalApiResponse();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(httpClient.BaseAddress, $"user/{email}/password-reset"),
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
                    Headers = { { "x-ranger-domain", domain } }
                };
            });
            apiResponse = await SendAsync(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return true;
            }
            if ((int)apiResponse.StatusCode == StatusCodes.Status400BadRequest)
            {
                return false;
            }
            throw new HttpClientException(apiResponse);
        }

        public async Task<bool> RequestEmailChange(string domain, string email, string jsonContent)
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

            var apiResponse = new InternalApiResponse();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(httpClient.BaseAddress, $"user/{email}/email-change"),
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
                    Headers = { { "x-ranger-domain", domain } }
                };
            });
            apiResponse = await SendAsync(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return true;
            }
            if ((int)apiResponse.StatusCode == StatusCodes.Status400BadRequest)
            {
                return false;
            }
            throw new HttpClientException(apiResponse);
        }

        public async Task<bool> ConfirmUserAsync(string domain, string userId, string jsonContent)
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

            var apiResponse = new InternalApiResponse();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(httpClient.BaseAddress, $"user/{userId}/confirm"),
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
                    Headers = { { "x-ranger-domain", domain } }
                };
            });
            apiResponse = await SendAsync(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return true;
            }
            if ((int)apiResponse.StatusCode == StatusCodes.Status304NotModified)
            {
                return false;
            }
            throw new HttpClientException(apiResponse);
        }

        public async Task<bool> UserConfirmPasswordResetAsync(string domain, string userId, string jsonContent)
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

            var apiResponse = new InternalApiResponse();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(httpClient.BaseAddress, $"user/{userId}/password-reset"),
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
                    Headers = { { "x-ranger-domain", domain } }
                };
            });
            apiResponse = await SendAsync(httpRequestMessageFactory);
            if (apiResponse.IsSuccessStatusCode)
            {
                return true;
            }
            if ((int)apiResponse.StatusCode == StatusCodes.Status304NotModified)
            {
                return false;
            }
            throw new HttpClientException(apiResponse);
        }

        public async Task UserConfirmEmailChangeAsync(string domain, string userId, string jsonContent)
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

            var apiResponse = new InternalApiResponse();
            Func<HttpRequestMessage> httpRequestMessageFactory = (() =>
            {
                return new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(httpClient.BaseAddress, $"user/{userId}/email-change"),
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
                    Headers = { { "x-ranger-domain", domain } }
                };
            });
            apiResponse = await SendAsync(httpRequestMessageFactory);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw new HttpClientException(apiResponse);
            }
        }
    }
}