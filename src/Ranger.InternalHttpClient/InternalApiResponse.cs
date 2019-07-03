using System.Collections.Generic;
using System.Net;

namespace Ranger.InternalHttpClient {
    public class InternalApiResponse<T> {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccessStatusCode { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public T ResponseObject { get; set; }
    }

    public class InternalApiResponse {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccessStatusCode { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}