namespace Ranger.InternalHttpClient
{

    public class ApiResponse<T>
    {
        public string Version { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
        public ResponseException ResponseException { get; set; }
        public T Result
        {
            get; set;
        }
    }

    public class ResponseException
    {
        public bool IsError { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string Detail { get; set; }
        public object Instance { get; set; }
        public object Extensions { get; set; }
        public ValidationError[] ValidationErrors { get; set; }

        public struct ValidationError
        {
            public string Name { get; set; }
            public string Reason { get; set; }
        }
    }
}