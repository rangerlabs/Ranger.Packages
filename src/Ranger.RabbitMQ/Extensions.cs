using System.Reflection;
using System.Threading;
using Autofac;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Ranger.Common;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.RabbitMQ.BusSubscriber;

namespace Ranger.RabbitMQ
{
    public static class Extensions
    {
        private const int ConnectionRetryDuration = 10000;
        private const int ConnectionMaxRetrys = 9;
        public static IBusSubscriber UseRabbitMQ(this IApplicationBuilder app) => app.ApplicationServices.GetRequiredService<IBusSubscriber>();

        public static void AddRabbitMq<TStartup>(this ContainerBuilder builder)
            where TStartup : class
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
                .InstancePerLifetimeScope();
            builder.RegisterType<BusPublisher.BusPublisher<TStartup>>().As<IBusPublisher>()
                .SingleInstance();
            builder.RegisterType<BusSubscriber.BusSubscriber>().As<IBusSubscriber>()
                .SingleInstance();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            RegisterConnection(builder);
        }

        private static void RegisterConnection(ContainerBuilder builder)
        {
            builder.Register<IConnection>(context =>
            {
                var options = context.Resolve<RabbitMQOptions>();
                var logger = context.Resolve<ILoggerFactory>().CreateLogger(typeof(Extensions));
                var connectionFactory = new ConnectionFactory() { DispatchConsumersAsync = true };
                connectionFactory.UserName = options.Username;
                connectionFactory.Password = options.Password;
                connectionFactory.HostName = options.Host;
                connectionFactory.Port = options.Port;
                connectionFactory.VirtualHost = options.VirtualHost;
                connectionFactory.AutomaticRecoveryEnabled = true;
                IConnection connection = default;

                bool connected = false;
                int connectionAttempt = 0;
                while (!connected && connectionAttempt <= ConnectionMaxRetrys)
                {
                    try
                    {
                        connection = connectionFactory.CreateConnection();
                    }
                    catch (BrokerUnreachableException ex)
                    {
                        logger.LogCritical(ex, $"Failed to connect to RabbitMQ broker. Retrying in {ConnectionRetryDuration / 1000} seconds. Connection attempt: {connectionAttempt}");
                        if (connectionAttempt == ConnectionMaxRetrys)
                        {
                            logger.LogCritical("Abandoning RabbitMQ connection");
                            throw;
                        }
                        connectionAttempt++;
                        Thread.Sleep(ConnectionRetryDuration);
                        continue;
                    }
                    connected = true;
                }
                logger.LogInformation("RabbitMQ connection established");
                return connection;
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