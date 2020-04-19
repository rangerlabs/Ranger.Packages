using System.Collections.Generic;

namespace Ranger.Common
{
    public class RangerApiError
    {
        public string Message { get; set; }
        public string Code { get; set; }

        public RangerApiError(string message, string code = "")
        {
            this.Message = message;
            this.Code = code;
        }
    }
}