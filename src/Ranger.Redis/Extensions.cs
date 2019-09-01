using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ranger.Common;

namespace Ranger.Redis
{
    public static class Extensions
    {
        private static readonly string SectionName = "redis";

        public static IServiceCollection AddRedis(this IServiceCollection services, ILogger logger = null)
        {
            IConfiguration configuration;
            using (var serviceProvider = services.BuildServiceProvider())
            {
                configuration = serviceProvider.GetService<IConfiguration>();
            }

            services.Configure<RedisOptions>(configuration.GetSection(SectionName));
            var options = configuration.GetOptions<RedisOptions>(SectionName);
            services.AddDistributedRedisCache(o =>
            {
                o.InstanceName = options.Instance;
                o.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
                {
                    AbortOnConnectFail = true,
                    Password = options.Password,
                    ClientName = options.Instance,
                    EndPoints = { { options.ConnectionString } },
                };
            });
            if (logger != null)
            {
                logger.LogInformation($"Connected to redis instance {options.Instance}:{options.ConnectionString}.");
            }
            return services;
        }
    }
}