using System;
using System.IdentityModel.Tokens.Jwt;
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
        private async Task InitializeHttpRequest(HttpRequestMessage httpRequestMessage)
        {
            var context = new Polly.Context().WithLogger(logger).WithHttpClientOptions(clientOptions).WithHttpClient(HttpClient).WithHttpRequestMessage(httpRequestMessage);
            httpRequestMessage.SetPolicyExecutionContext(context);
            if (!String.IsNullOrWhiteSpace(clientOptions.Token) && !tokenIsExpired())
            {
                logger.LogDebug("The existing access token is not expired, reusing existing access token");
                httpRequestMessage.SetBearerToken(clientOptions.Token);
            }
            else
            {
                logger.LogDebug("No token was found or the existing access token is expired, requesting a new access token");
                await httpRequestMessage.SetNewClientToken(HttpClient, this.clientOptions, this.logger);
            }
        }

        ///<summary>
        ///This isn't meant to be perfect but will reduce unnecessary token requests significantly, let Polly handle the in-betweens
        ///</summary>
        private bool tokenIsExpired()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(clientOptions.Token);
            if (token.ValidTo > DateTime.UtcNow)
            {
                return false;
            }
            return true;
        }

        ///<summary>
        /// Makes an HTTP request with no expected result
        /// Throws an ApiException on 5XX responses
        ///</summary>
        protected async Task<RangerApiResponse> SendAsync(HttpRequestMessage httpRequestMessage)
        {
            await InitializeHttpRequest(httpRequestMessage);
            HttpResponseMessage response = null;
            if (!String.IsNullOrWhiteSpace(clientOptions.Token) && !tokenIsExpired())
            {
                httpRequestMessage.SetBearerToken(clientOptions.Token);
            }
            else
            {
                await httpRequestMessage.SetNewClientToken(HttpClient, this.clientOptions, this.logger);
            }

            try
            {
                response = await HttpClient.SendAsync(httpRequestMessage);
            }
            catch (HttpRequestException ex)
            {
                var message = "The request failed after executing all policies";
                logger.LogError(ex, message);
                throw new ApiException(Constants.ExceptionMessage, statusCode: StatusCodes.Status500InternalServerError);
            }
            logger.LogDebug("Received status code {StatusCode}", response.StatusCode);
            var content = await response.Content?.ReadAsStringAsync() ?? "";

            if (String.IsNullOrWhiteSpace(content))
            {
                logger.LogCritical("The response body was empty when a response was intended");
                throw new ApiException(Constants.ExceptionMessage, statusCode: StatusCodes.Status500InternalServerError);
            }

            try
            {
                var rangerApiResponse = JsonConvert.DeserializeObject<RangerApiResponse>(content, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore });
                if (rangerApiResponse.IsError)
                {
                    throw new ApiException(rangerApiResponse.Error.Message, statusCode: rangerApiResponse.StatusCode) { Errors = rangerApiResponse.Error.ValidationErrors ?? default };
                }
                return rangerApiResponse;
            }
            catch (JsonSerializationException ex)
            {
                logger.LogCritical(ex, "The http client failed to deserialize an APIs response. The response body contained the following: {ResponseBody}", content);
                throw new ApiException(Constants.ExceptionMessage, statusCode: StatusCodes.Status500InternalServerError);
            }
        }



        ///<summary>
        /// Makes an HTTP request and deserializes the result to the specified type
        /// Throws an ApiException on 5XX responses
        ///</summary>
        protected async Task<RangerApiResponse<TResponseObject>> SendAsync<TResponseObject>(HttpRequestMessage httpRequestMessage)
        {
            await InitializeHttpRequest(httpRequestMessage);
            HttpResponseMessage response = null;
            try
            {
                response = await HttpClient.SendAsync(httpRequestMessage);
            }
            catch (HttpRequestException ex)
            {
                var message = "The request failed after executing all policies";
                logger.LogError(ex, message);
                throw new ApiException(Constants.ExceptionMessage, statusCode: StatusCodes.Status500InternalServerError);
            }
            logger.LogDebug("Received status code {StatusCode}", response.StatusCode);
            var content = await response.Content?.ReadAsStringAsync() ?? "";

            if (String.IsNullOrWhiteSpace(content))
            {
                logger.LogCritical("The response body was empty when a response was intended");
                throw new ApiException(Constants.ExceptionMessage, statusCode: StatusCodes.Status500InternalServerError);
            }
            try
            {
                var rangerApiResponse = JsonConvert.DeserializeObject<RangerApiResponse<TResponseObject>>(content, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore });
                if (rangerApiResponse.IsError)
                {
                    throw new ApiException(rangerApiResponse.Error.Message, statusCode: rangerApiResponse.StatusCode) { Errors = rangerApiResponse.Error.ValidationErrors ?? default };
                }
                return rangerApiResponse;
            }
            catch (JsonSerializationException ex)
            {
                logger.LogCritical(ex, "The http client failed to deserialize an APIs response. The response body contained the following: {ResponseBody}", content);
                throw new ApiException(Constants.ExceptionMessage, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}