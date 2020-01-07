using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDb.Bson.NodaTime;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Ranger.Common;

namespace Ranger.Mongo
{
    public class MongoDbInitializer : IMongoDbInitializer
    {
        private static bool _initialized;
        private readonly IMongoDatabase _database;
        private readonly IMongoDbSeeder _seeder;

        public MongoDbInitializer(IMongoDatabase database,
            IMongoDbSeeder seeder,
            MongoDbOptions options
            )
        {
            _database = database;
            _seeder = seeder;
        }

        public async Task Initialize()
        {
            if (_initialized)
            {
                return;
            }
            RegisterConventions();
            _initialized = true;
            await _seeder.SeedAsync();
        }

        private void RegisterConventions()
        {
            NodaTimeSerializers.Register();
            BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
            BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
            ConventionRegistry.Register("Conventions", new MongoDbConventions(), x => true);
        }


        private class MongoDbConventions : IConventionPack
        {
            public IEnumerable<IConvention> Conventions => new List<IConvention>
            {
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
                new CamelCaseElementNameConvention()
            };
        }
    }
}