using System;
using AutoWrapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.IO;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace Ranger.ApiUtilities
{
    public static class Extensions
    {
        public static IServiceCollection AddAutoWrapper(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            return services;
        }

        public static IApplicationBuilder UseAutoWrapper(this IApplicationBuilder app, string wrapWhenApiPathStartsWith = "")
        {
            var autoWrapperOptions = new AutoWrapperOptions();
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
            {
                autoWrapperOptions.IsDebug = true;
                autoWrapperOptions.EnableResponseLogging = true;
                autoWrapperOptions.EnableExceptionLogging = true;
            }

            autoWrapperOptions.ShowStatusCode = true;
            autoWrapperOptions.IsApiOnly = true;
            autoWrapperOptions.WrapWhenApiPathStartsWith = wrapWhenApiPathStartsWith;

            app.UseApiResponseAndExceptionWrapper(autoWrapperOptions);
            return app;
        }

        public static IServiceCollection AddSwaggerGen(this IServiceCollection services, string title, string version)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException($"{nameof(title)} was null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentException($"{nameof(version)} was null or whitespace");
            }

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(version, new OpenApiInfo
                {
                    Version = version,
                    Title = title
                });

                var xmlFile = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            return services;
        }

        public static IApplicationBuilder UseSwagger(this IApplicationBuilder app, string version, string name)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentException($"{nameof(version)} was null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{nameof(name)} was null or whitespace");
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
             {
                 c.SwaggerEndpoint($"/swagger/{version}/swagger.json", name);
             });
            return app;
        }
    }
}