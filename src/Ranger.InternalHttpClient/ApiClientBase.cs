using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;

namespace Ranger.InternalHttpClient
{
    public class ApiClientBase
    {
        private readonly ILogger logger;
        internal HttpClient HttpClient { get; }

        public ApiClientBase(HttpClient httpClient, ILogger logger)
        {
            HttpClient = httpClient;
            this.logger = logger;
        }

        protected async Task<RangerApiResponse> SendAsync(HttpRequestMessage httpRequestMessage)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await HttpClient.SendAsync(httpRequestMessage);
            }
            catch (HttpRequestException ex)
            {
                var message = "The request failed after executing all policies";
                logger.LogError(ex, message);
                return new RangerApiResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsError = true,
                    Message = ex.Message,
                    ResponseException = new ResponseException
                    {
                        ExceptionMessage = new ExceptionMessage
                        {
                            Error = new Error
                            {
                                Message = "An Internal Server Error occurred",
                            }
                        }
                    }

                };
            }
            logger.LogDebug("Received status code {StatusCode}", response.StatusCode);
            var content = await response.Content?.ReadAsStringAsync() ?? "";

            if (String.IsNullOrWhiteSpace(content))
            {
                throw new HttpClientException($"The response body was empty");
            }
            try
            {
                return JsonConvert.DeserializeObject<RangerApiResponse>(content, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                });
            }
            catch (JsonSerializationException ex)
            {
                throw new HttpClientException($"Failed to deserialize response body", ex);
            }
        }

        protected async Task<RangerApiResponse<TResponseObject>> SendAsync<TResponseObject>(HttpRequestMessage httpRequestMessage)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await HttpClient.SendAsync(httpRequestMessage);
            }
            catch (HttpRequestException ex)
            {
                var message = "The request failed after executing all policies";
                logger.LogError(ex, message);
                return new RangerApiResponse<TResponseObject>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsError = true,
                    Message = ex.Message,
                    ResponseException = new ResponseException
                    {
                        ExceptionMessage = new ExceptionMessage
                        {
                            Error = new Error
                            {
                                Message = "An Internal Server Error occurred",
                            }
                        }
                    }
                };
            }
            logger.LogDebug("Received status code {StatusCode}", response.StatusCode);
            var content = await response.Content?.ReadAsStringAsync() ?? "";

            if (String.IsNullOrWhiteSpace(content))
            {
                throw new HttpClientException($"The response body was empty");
            }
            try
            {
                return JsonConvert.DeserializeObject<RangerApiResponse<TResponseObject>>(content, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore });
            }
            catch (JsonSerializationException ex)
            {
                throw new HttpClientException($"Failed to deserialize response body", ex);
            }
        }
    }
}
