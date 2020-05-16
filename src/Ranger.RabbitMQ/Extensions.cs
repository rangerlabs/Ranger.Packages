using System;
using System.Reflection;
using Autofac;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using RabbitMQ.Client;
using Ranger.Common;

namespace Ranger.RabbitMQ
{
    public static class Extensions
    {
        public static IBusSubscriber UseRabbitMQ(this IApplicationBuilder app) => new BusSubscriber(app);

        public static void AddRabbitMq(this ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                var options = configuration.GetOptions<RabbitMQOptions>("rabbitMQ");
                return options;
            }).SingleInstance();

            var assembly = Assembly.GetCallingAssembly();
            builder.RegisterAssemblyTypes(assembly)
                .AsClosedTypesOf(typeof(IMessageHandler<>))
                .InstancePerDependency();
            builder.RegisterType<BusPublisher>().As<IBusPublisher>()
                .SingleInstance();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            RegisterConnectionFactory(builder);
        }

        private static void RegisterConnectionFactory(ContainerBuilder builder)
        {
            builder.Register<IConnection>(context =>
            {
                var options = context.Resolve<RabbitMQOptions>();
                var connectionFactory = new ConnectionFactory() { DispatchConsumersAsync = true };
                connectionFactory.UserName = options.Username;
                connectionFactory.Password = options.Password;
                connectionFactory.HostName = options.Host;
                connectionFactory.Port = options.Port;
                connectionFactory.VirtualHost = options.VirtualHost;
                connectionFactory.AutomaticRecoveryEnabled = true;
                return connectionFactory.CreateConnection();
            }).SingleInstance();
        }

        public static IServiceCollection AddRabbitMQHealthCheck(this IServiceCollection services)
        {
            services.AddHealthChecks().AddRabbitMQ(sp => sp.GetService<IConnection>(), "rabbit-mq", tags: new string[] { "rabbit-mq" });
            return services;
        }
        public static void MapRabbitMQHealthCheck(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health-checks/rabbit-mq", new HealthCheckOptions
            {
                Predicate = (check) => check.Tags.Contains("rabbit-mq"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }
    }
}