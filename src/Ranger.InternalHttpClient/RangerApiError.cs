using System.Collections.Generic;
using AutoWrapper.Wrappers;

namespace Ranger.InternalHttpClient
{
    public class RangerApiError
    {
        public string Message { get; set; }
        public IEnumerable<ValidationError> ValidationErrors { get; set; }
    }
}