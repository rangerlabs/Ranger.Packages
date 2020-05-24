using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ranger.Monitoring.HealthChecks
{
    public static class Extensions
    {
        public static IServiceCollection AddDockerImageTagHealthCheck(this IServiceCollection services)
        {
            services.AddHealthChecks().AddCheck<DockerImageTagHealthCheck>("docker-image-tag");
            return services;
        }
        public static IServiceCollection AddEntityFrameworkHealthCheck<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            services.AddHealthChecks().AddDbContextCheck<TContext>("ef-core");
            return services;
        }
        public static IServiceCollection AddLiveHealthCheck(this IServiceCollection services)
        {
            services.AddHealthChecks().AddCheck("live", () => HealthCheckResult.Healthy());
            return services;
        }

        public static void MapHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health-checks", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }

        public static void MapDockerImageTagHealthCheck(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health-checks/docker-image-tag", new HealthCheckOptions
            {
                Predicate = (check) => check.Name.Equals("docker-image-tag"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }

        public static void MapLiveTagHealthCheck(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health-checks/live", new HealthCheckOptions
            {
                Predicate = (check) => check.Name.Equals("live"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }
        public static void MapEfCoreTagHealthCheck(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health-checks/ef-core", new HealthCheckOptions
            {
                Predicate = (check) => check.Name.Equals("ef-core"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }

    }
}