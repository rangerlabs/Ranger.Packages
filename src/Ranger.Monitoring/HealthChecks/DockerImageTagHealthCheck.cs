using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ranger.Monitoring.HealthChecks
{
    public class DockerImageTagHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new HealthCheckResult(
                HealthStatus.Healthy,
                description: "Reports the docker image tag of the container",
                exception: null,
                data: new Dictionary<string, object> { { "Image Tag", Environment.GetEnvironmentVariable("DOCKER_IMAGE_TAG") } }
            ));
        }
    }
}