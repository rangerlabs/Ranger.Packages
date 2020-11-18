using System.Net.Http.Headers;

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
        public HttpResponseHeaders Headers { get; set; }
    }

    public class RangerApiResponse<T> : IRangerApiResponse
    {
        public string Version { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
        public RangerApiError Error { get; set; }
        public T Result { get; set; }
        public HttpResponseHeaders Headers { get; set; }
    }
}