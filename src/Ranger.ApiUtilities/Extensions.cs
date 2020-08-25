using System;
using AutoWrapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.IO;
using Microsoft.OpenApi.Models;
using AutoWrapper.Wrappers;
using AutoWrapper.Extensions;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Logging;

namespace Ranger.ApiUtilities
{
    public static class Extensions
    {
        public static IMvcBuilder AddRangerFluentValidation<T>(this IMvcBuilder mvcBuilder)
        {
            mvcBuilder.AddFluentValidation(options =>
                {
                    options.ConfigureClientsideValidation(enabled: false);
                    options.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                    options.RegisterValidatorsFromAssemblyContaining<T>();
                });
            return mvcBuilder;
        }

        public static IServiceCollection AddRangerApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.ApiVersionReader = new HeaderApiVersionReader("api-version");
                o.ErrorResponses = new ApiVersionResponseProvider();
            });
            return services;
        }

        public static IServiceCollection ConfigureAutoWrapperModelStateResponseFactory(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    throw new ApiException(context.ModelState.AllErrors());
                };
            });
            return services;
        }

        public static IApplicationBuilder UseAutoWrapper(this IApplicationBuilder app, bool isApiOnly = true, string wrapWhenApiPathStartsWith = "")
        {
            var autoWrapperOptions = new AutoWrapperOptions();
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
            {
                autoWrapperOptions.IsDebug = true;
                autoWrapperOptions.EnableResponseLogging = true;
                autoWrapperOptions.EnableExceptionLogging = true;
            }
            else
            {
                autoWrapperOptions.IsDebug = false;
                autoWrapperOptions.EnableResponseLogging = false;
                autoWrapperOptions.EnableExceptionLogging = false;
            }

            autoWrapperOptions.ShowStatusCode = true;
            autoWrapperOptions.IsApiOnly = isApiOnly;
            autoWrapperOptions.WrapWhenApiPathStartsWith = wrapWhenApiPathStartsWith;

            app.UseApiResponseAndExceptionWrapper<MapResponseObject>(autoWrapperOptions);
            return app;
        }

        public static IApplicationBuilder UseUnhandedExceptionLogger(this IApplicationBuilder app)
        {
            app.UseMiddleware<UnhandledExceptionLogger>();
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