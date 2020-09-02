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
}