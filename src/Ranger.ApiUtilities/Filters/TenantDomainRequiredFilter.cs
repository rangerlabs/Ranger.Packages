using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Ranger.InternalHttpClient;

namespace Ranger.ApiUtilities
{
    public class TenantDomainRequiredAttribute : TypeFilterAttribute
    {
        public TenantDomainRequiredAttribute() : base(typeof(TenantDomainRequiredFilterImpl)) { }
        private class TenantDomainRequiredFilterImpl : IAsyncActionFilter
        {
            private readonly ITenantsClient tenantsClient;
            private ILogger<TenantDomainRequiredFilterImpl> logger { get; }

            public TenantDomainRequiredFilterImpl(ITenantsClient tenantsClient, ILogger<TenantDomainRequiredFilterImpl> logger)
            {
                this.tenantsClient = tenantsClient;
                this.logger = logger;
            }
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                StringValues domain;
                bool success = context.HttpContext.Request.Headers.TryGetValue("x-ranger-domain", out domain);
                if (success)
                {
                    if (domain.Count == 1)
                    {
                        try
                        {
                            var tenantApiResponse = await tenantsClient.EnabledAsync<EnabledResult>(domain);
                            if (tenantApiResponse.Enabled)
                            {
                                await next();
                            }
                            else
                            {
                                context.Result = new ForbidResult($"The tenant for the provided x-ranger-domain header is not enabled '{domain}'. Ensure the domain has been confirmed.");
                                return;
                            }
                        }
                        catch (HttpClientException ex)
                        {
                            if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                            {
                                context.Result = new NotFoundObjectResult($"No tenant found for the provided x-ranger-domain header '{domain}'.");
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError(ex, $"An exception occurred validating whether the domain '{domain}' exists.");
                            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                            return;
                        }
                    }
                    else
                    {
                        context.Result = new BadRequestObjectResult(new { errors = new { serverErrors = "Multiple x-ranger-domain header values were found for x-ranger-domain." } });
                        return;
                    }
                }
                else
                {
                    context.Result = new BadRequestObjectResult(new { errors = new { serverErrors = "No x-ranger-domain header value was found." } });
                    return;
                }
            }
        }
    }
}