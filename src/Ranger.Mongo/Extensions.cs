using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
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

                return new MongoClient(options.ConnectionString);

            }).SingleInstance();

            builder.Register(context =>
                {
                    var loggerFactory = context.Resolve<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("Ranger.Mongo.Extensions");
                    var options = context.Resolve<MongoDbOptions>();
                    var client = context.Resolve<MongoClient>();
                    logger.LogInformation($"Creating new MongoDatabase '{options.DefaultDatabase}'.");
                    return client.GetDatabase(options.DefaultDatabase);
                }).InstancePerLifetimeScope();

            builder.RegisterType<MongoDbInitializer>()
                        .As<IMongoDbInitializer>()
                        .InstancePerLifetimeScope();
        }
    }
}
