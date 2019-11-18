// using System;
// using System.Net;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Filters;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Primitives;
// using Ranger.Common;
// using Ranger.InternalHttpClient;

// namespace Ranger.ApiUtilities
// {
//     public class InvalidModelStateResponseAttribute : TypeFilterAttribute
//     {
//         public InvalidModelStateResponseAttribute() : base(typeof(InvalidModelStateResponseFilterImpl)) { }
//         private class InvalidModelStateResponseFilterImpl : IAsyncActionFilter
//         {
//             private readonly ITenantsClient tenantsClient;
//             private ILogger<InvalidModelStateResponseFilterImpl> logger { get; }

//             public InvalidModelStateResponseFilterImpl(ITenantsClient tenantsClient, ILogger<InvalidModelStateResponseFilterImpl> logger)
//             {
//                 this.tenantsClient = tenantsClient;
//                 this.logger = logger;
//             }

//             public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
//             {
//                 if (context.ModelState.IsValid == false)
//                 {
//                     var apiErrorContent = new ApiErrorContent();
//                     foreach(var error in context.ModelState)
//                     {
//                         apiErrorContent.Errors.Add($"{error.Key}: {error.Value.})
//                     }
//                     context.Result = context.HttpContext.Request.CreateErrorResponse(
//                         HttpStatusCode.BadRequest, context.ModelState);
//                 }
//             }
//         }
//     }
// }