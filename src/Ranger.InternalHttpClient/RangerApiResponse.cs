namespace Ranger.InternalHttpClient
{
    public class RangerApiResponse : IRangerApiResponse
    {
        public string Version { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
        public ResponseException ResponseException { get; set; }
        public object Result { get; set; }
    }

    public class RangerApiResponse<T> : IRangerApiResponse
    {
        public string Version { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
        public ResponseException ResponseException { get; set; }
        public T Result { get; set; }
    }
}