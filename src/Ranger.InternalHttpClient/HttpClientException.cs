using System;
using System.Runtime.Serialization;

namespace Ranger.InternalHttpClient
{
    public class HttpClientException : Exception
    {
        public InternalApiResponse ApiResponse { get; }

        public HttpClientException(InternalApiResponse apiResponse)
        {
            ApiResponse = apiResponse ?? throw new ArgumentNullException(nameof(apiResponse));
        }

        public HttpClientException(InternalApiResponse apiResponse, string message) : base(message)
        {
            ApiResponse = apiResponse ?? throw new ArgumentNullException(nameof(apiResponse));
        }

        public HttpClientException(InternalApiResponse apiResponse, string message, Exception innerException) : base(message, innerException)
        {
            ApiResponse = apiResponse ?? throw new ArgumentNullException(nameof(apiResponse));
        }

        protected HttpClientException(InternalApiResponse apiResponse, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ApiResponse = apiResponse ?? throw new ArgumentNullException(nameof(apiResponse));
        }
    }

    public class HttpClientException<T> : Exception
    {
        public InternalApiResponse<T> ApiResponse { get; }

        public HttpClientException(InternalApiResponse<T> apiResponse)
        {
            ApiResponse = apiResponse ?? throw new ArgumentNullException(nameof(apiResponse));
        }

        public HttpClientException(InternalApiResponse<T> apiResponse, string message) : base(message)
        {
            ApiResponse = apiResponse ?? throw new ArgumentNullException(nameof(apiResponse));
        }

        public HttpClientException(InternalApiResponse<T> apiResponse, string message, Exception innerException) : base(message, innerException)
        {
            ApiResponse = apiResponse ?? throw new ArgumentNullException(nameof(apiResponse));
        }

        protected HttpClientException(InternalApiResponse<T> apiResponse, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ApiResponse = apiResponse ?? throw new ArgumentNullException(nameof(apiResponse));
        }
    }
}