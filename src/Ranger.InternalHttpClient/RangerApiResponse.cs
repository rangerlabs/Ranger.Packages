using System;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Http;
using Ranger.Common;

namespace Ranger.InternalHttpClient
{
    public class RangerApiResponse : IRangerApiResponse
    {
        public string Version { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
        public RangerApiError Error { get; set; }
        public object Result { get; set; }

    }

    public class RangerApiResponse<T> : IRangerApiResponse
    {
        public string Version { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
        public RangerApiError Error { get; set; }
        public T Result { get; set; }
    }

    public static class RangerApiResponseExtensions
    {
        public static ApiResponse ToApiResponse(this RangerApiResponse rangerApiResponse)
        {
            if (rangerApiResponse.StatusCode >= StatusCodes.Status500InternalServerError)
            {
                throw new ArgumentException("Status codes greater than 500 must throw ApiExceptions. Cannot convert to ApiResponse");
            }
            return new ApiResponse(rangerApiResponse.Message, rangerApiResponse.Result, rangerApiResponse.StatusCode);
        }
        public static ApiResponse ToApiResponse<T>(this RangerApiResponse<T> rangerApiResponse)
        {
            if (rangerApiResponse.StatusCode >= StatusCodes.Status500InternalServerError)
            {
                throw new ArgumentException("Status codes greater than 500 must throw ApiExceptions. Cannot convert to ApiResponse");
            }
            return new ApiResponse(rangerApiResponse.Message, rangerApiResponse.Result, rangerApiResponse.StatusCode);
        }
        public static bool Is5XXStatusCode(this RangerApiResponse rangerApiResponse) => rangerApiResponse.StatusCode >= StatusCodes.Status500InternalServerError;
        public static bool Is5XXStatusCode<T>(this RangerApiResponse<T> rangerApiResponse) => rangerApiResponse.StatusCode >= StatusCodes.Status500InternalServerError;
        public static bool Is4XXStatusCode(this RangerApiResponse rangerApiResponse) => rangerApiResponse.StatusCode >= StatusCodes.Status400BadRequest;
        public static bool Is4XXStatusCode<T>(this RangerApiResponse<T> rangerApiResponse) => rangerApiResponse.StatusCode >= StatusCodes.Status400BadRequest;
    }
}