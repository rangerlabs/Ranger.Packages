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

            public ILogger<TenantDomainRequiredFilterImpl> logger { get; }

            public TenantDomainRequiredFilterImpl(ITenantsClient tenantsClient, ILogger<TenantDomainRequiredFilterImpl> logger)
            {
                this.tenantsClient = tenantsClient;
                this.logger = logger;
            }
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {

                StringValues domain;
                bool success = context.HttpContext.Request.Headers.TryGetValue("X-Tenant-Domain", out domain);
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
                                context.Result = new ForbidResult($"The tenant for the provided X-Tenant-Domain header is not enabled '{domain}'. Ensure the domain has been confirmed.");
                            }
                        }
                        catch (HttpClientException ex)
                        {
                            if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                            {
                                context.Result = new NotFoundObjectResult($"No tenant found for the provided X-Tenant-Domain header '{domain}'.");
                            }
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError(ex, $"An exception occurred validating whether the domain '{domain}' exists.");
                            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                        }
                    }
                    else
                    {
                        context.Result = new BadRequestObjectResult(new { errors = new { serverErrors = "Multiple X-Tenant-Domain header values were found for X-Tenant-Domain." } });
                    }
                }
                else
                {
                    context.Result = new BadRequestObjectResult(new { errors = new { serverErrors = "No X-Tenant-Domain header value was found." } });
                }
            }
        }
    }
}