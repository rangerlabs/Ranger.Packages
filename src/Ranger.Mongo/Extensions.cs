using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Ranger.Common;

namespace Ranger.Mongo
{
    public static class Extensions
    {
        public static void AddMongo(this ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                var options = configuration.GetOptions<MongoDbOptions>("mongo");
                return options;
            }).SingleInstance();

            builder.Register(context =>
            {
                var loggerFactory = context.Resolve<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Ranger.Mongo.Extensions");
                var options = context.Resolve<MongoDbOptions>();

                logger.LogInformation($"Adding MongoCredential for user '{options.Username}' with password '{options.Password}' on database '{options.Database}'.");
                var clientSettings = new MongoClientSettings()
                {
                    Credential = MongoCredential.CreateCredential(options.Database, options.Username, options.Password),
                    Server = MongoServerAddress.Parse(options.ConnectionString),
                };

                logger.LogInformation($"Creating new MongoClient for '{options.ConnectionString}'.");
                return new MongoClient(clientSettings);
            }).SingleInstance();

            builder.Register(context =>
                {
                    var loggerFactory = context.Resolve<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("Ranger.Mongo.Extensions");
                    var options = context.Resolve<MongoDbOptions>();
                    var client = context.Resolve<MongoClient>();
                    logger.LogInformation($"Creating new MongoDatabase '{options.Database}'.");
                    return client.GetDatabase(options.Database);

                }).InstancePerLifetimeScope();

            builder.RegisterType<MongoDbInitializer>()
                        .As<IMongoDbInitializer>()
                        .InstancePerLifetimeScope();
        }
    }
}
