using System;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ranger.ApiUtilities
{
    public class UnhandledExceptionLogger
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UnhandledExceptionLogger> logger;

        public UnhandledExceptionLogger(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            logger = loggerFactory.CreateLogger<UnhandledExceptionLogger>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An unhandled exception occurred");
                throw;
            }
        }
    }
}