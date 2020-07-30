using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace Ranger.ApiUtilities
{
    class ApiVersionResponseProvider : DefaultErrorResponseProvider
    {
        public override IActionResult CreateResponse(ErrorResponseContext context)
        {
            switch (context.ErrorCode)
            {
                case "UnsupportedApiVersion":
                    return new BadRequestObjectResult("Unsupported API version.");
                case "ApiVersionUnspecified":
                    return new BadRequestObjectResult("API version unspecified.");
            }
            return base.CreateResponse(context);
        }
    }
}