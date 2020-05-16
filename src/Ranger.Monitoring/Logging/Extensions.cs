using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Ranger.Common;
using Serilog;

namespace Ranger.Monitoring.Logging
{
    public static class Extensions
    {
        public static IHostBuilder UseLogging(this IHostBuilder hostBuilder, string applicationName = null) => hostBuilder.UseSerilog((context, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(context.Configuration);
            var appOptions = context.Configuration.GetOptions<AppOptions>("app");
            applicationName = string.IsNullOrWhiteSpace(applicationName) ? appOptions.Name : applicationName;
            loggerConfiguration.Enrich.FromLogContext()
                            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                            .Enrich.WithProperty("Application", applicationName);
        });

        public static IWebHostBuilder UseLogging(this IWebHostBuilder webHostBuilder, string applicationName = null) => webHostBuilder.UseSerilog((context, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(context.Configuration);
            var appOptions = context.Configuration.GetOptions<AppOptions>("app");
            applicationName = string.IsNullOrWhiteSpace(applicationName) ? appOptions.Name : applicationName;
            loggerConfiguration.Enrich.FromLogContext()
                              .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                              .Enrich.WithProperty("Application", applicationName);
        });
    }
}