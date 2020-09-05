using System;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Ranger.Redis
{
    public static class Extensions
    {
        public static IServiceCollection AddRedis(this IServiceCollection services, string connectionString, out IConnectionMultiplexer connectionMultiplexer)
        {
            connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
            return services;
        }
    }
}
