using System.Collections.Generic;
using System.Net;
using Ranger.Common;

namespace Ranger.InternalHttpClient
{
    public class InternalApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccessStatusCode { get; set; }
        public ApiErrorContent Errors { get; set; }
        public T ResponseObject { get; set; }
    }

    public class InternalApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccessStatusCode { get; set; }
        public ApiErrorContent Errors { get; set; }
    }
}