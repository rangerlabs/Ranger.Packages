using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Ranger.Common;

namespace Ranger.InternalHttpClient
{
    public class ApiClientBase
    {
        private readonly IHttpClientOptions clientOptions;
        private readonly ILogger logger;
        protected readonly HttpClient HttpClient;

        public ApiClientBase(HttpClient httpClient, IHttpClientOptions clientOptions, ILogger logger)
        {
            this.HttpClient = httpClient;
            this.HttpClient.BaseAddress = new Uri(clientOptions.BaseUrl);
            this.HttpClient.DefaultRequestHeaders.Add("api-version", "1.0");
            this.clientOptions = clientOptions;
            this.logger = logger;
        }

        ///<summary>
        /// Initialize the httpClient from the factory and wrap it with the necessary policies
        ///</summary>
        private void InitializeHttpRequest(HttpRequestMessage httpRequestMessage)
        {
            var context = new Polly.Context().WithLogger(logger).WithHttpClientOptions(clientOptions).WithHttpClient(HttpClient);
            httpRequestMessage.SetPolicyExecutionContext(context);
        }

        ///<summary>
        /// Makes an HTTP request with no expected result
        /// Throws an ApiException on 5XX responses
        ///</summary>
        protected async Task<RangerApiResponse> SendAsync(HttpRequestMessage httpRequestMessage)
        {
            InitializeHttpRequest(httpRequestMessage);
            HttpResponseMessage response = null;
            try
            {
                response = await HttpClient.SendAsync(httpRequestMessage);
            }
            catch (HttpRequestException ex)
            {
                var message = "The request failed after executing all policies";
                logger.LogError(ex, message);
                throw new ApiException(new RangerApiError("An internal server error occurred"));
            }
            logger.LogDebug("Received status code {StatusCode}", response.StatusCode);
            var content = await response.Content?.ReadAsStringAsync() ?? "";

            if (String.IsNullOrWhiteSpace(content))
            {
                logger.LogCritical("The response body was empty when a response was intended");
                throw new ApiException($"An internal server error occurred");
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
                logger.LogCritical(ex, "The http client failed to deserialize an APIs response");
                throw new ApiException($"An internal server error occurred");
            }
        }

        ///<summary>
        /// Makes an HTTP request and deserializes the result to the specified type
        /// Throws an ApiException on 5XX responses
        ///</summary>
        protected async Task<RangerApiResponse<TResponseObject>> SendAsync<TResponseObject>(HttpRequestMessage httpRequestMessage)
        {
            InitializeHttpRequest(httpRequestMessage);
            HttpResponseMessage response = null;
            try
            {
                response = await HttpClient.SendAsync(httpRequestMessage);
            }
            catch (HttpRequestException ex)
            {
                var message = "The request failed after executing all policies";
                logger.LogError(ex, message);
                throw new ApiException(new RangerApiError("An internal server error occurred"));
            }
            logger.LogDebug("Received status code {StatusCode}", response.StatusCode);
            var content = await response.Content?.ReadAsStringAsync() ?? "";

            if (String.IsNullOrWhiteSpace(content))
            {
                logger.LogCritical("The response body was empty when a response was intended");
                throw new ApiException($"An internal server error occurred");
            }
            try
            {
                var rangerApiResponse = JsonConvert.DeserializeObject<RangerApiResponse<TResponseObject>>(content, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore });
                if (rangerApiResponse.Is5XXStatusCode())
                {
                    throw new ApiException(rangerApiResponse.Error, statusCode: rangerApiResponse.StatusCode);
                }
                return rangerApiResponse;
            }
            catch (JsonSerializationException ex)
            {
                logger.LogCritical(ex, "The http client failed to deserialize an APIs response");
                throw new ApiException($"An internal server error occurred");
            }
        }
    }
}