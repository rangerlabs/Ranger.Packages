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
        public ExceptionMessage ExceptionMessage { get; set; }
        public ValidationError[] ValidationErrors { get; set; }

        public struct ValidationError
        {
            public string Name { get; set; }
            public string Reason { get; set; }
        }
    }

    public class ExceptionMessage
    {
        public Error Error { get; set; }
    }

    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public Error InnerError { get; set; }
    }
}