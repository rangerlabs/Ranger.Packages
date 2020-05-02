using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Ranger.Common;
using Serilog;
using Serilog.Events;

namespace Ranger.Logging
{
    public static class Extensions
    {
        public static IHostBuilder UseLogging(this IHostBuilder hostBuilder, string applicationName = null) => hostBuilder.UseSerilog((context, loggerConfiguration) =>
        {
            var appOptions = context.Configuration.GetOptions<AppOptions>("app");
            var seqOptions = context.Configuration.GetOptions<SeqOptions>("seq");
            var serilogOptions = context.Configuration.GetOptions<SerilogOptions>("serilog");
            if (!Enum.TryParse<LogEventLevel>(serilogOptions.Level, true, out var level))
            {
                level = LogEventLevel.Information;
            }

            applicationName = string.IsNullOrWhiteSpace(applicationName) ? appOptions.Name : applicationName;
            loggerConfiguration.Enrich.FromLogContext()
                            .MinimumLevel.Is(level)
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                            .MinimumLevel.Override("System", LogEventLevel.Error)
                            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                            .Enrich.WithProperty("ApplicationName", applicationName);
            ConfigureOutput(loggerConfiguration, seqOptions, serilogOptions);
        });

        public static IWebHostBuilder UseLogging(this IWebHostBuilder webHostBuilder, string applicationName = null) => webHostBuilder.UseSerilog((context, loggerConfiguration) =>
        {
            var appOptions = context.Configuration.GetOptions<AppOptions>("app");
            var seqOptions = context.Configuration.GetOptions<SeqOptions>("seq");
            var serilogOptions = context.Configuration.GetOptions<SerilogOptions>("serilog");
            if (!Enum.TryParse<LogEventLevel>(serilogOptions.Level, true, out var level))
            {
                level = LogEventLevel.Information;
            }

            applicationName = string.IsNullOrWhiteSpace(applicationName) ? appOptions.Name : applicationName;
            loggerConfiguration.Enrich.FromLogContext()
                            .MinimumLevel.Is(level)
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                            .MinimumLevel.Override("System", LogEventLevel.Error)
                            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                            .Enrich.WithProperty("ApplicationName", applicationName);
            ConfigureOutput(loggerConfiguration, seqOptions, serilogOptions);
        });

        private static void ConfigureOutput(LoggerConfiguration loggerConfiguration, SeqOptions seqOptions, SerilogOptions serilogOptions)
        {
            if (seqOptions.Enabled)
            {
                loggerConfiguration.WriteTo.Seq(seqOptions.Url, apiKey: seqOptions.ApiKey);
            }

            if (serilogOptions.ConsoleEnabled)
            {
                loggerConfiguration.WriteTo.Console();
            }
        }
    }
}