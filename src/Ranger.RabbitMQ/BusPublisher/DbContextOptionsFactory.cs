using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ranger.RabbitMQ.BusPublisher
{
    internal static class DbContextOptionsFactory
    {
        public static DbContextOptions<TDbContext> GetDbContextOptions<TDbContext>(IConfiguration configuration, ILoggerFactory loggerFactory)
            where TDbContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TDbContext>();
            builder.UseNpgsql(configuration["cloudSql:ConnectionString"]);
            builder.UseLoggerFactory(loggerFactory);
            return builder.Options;
        }
    }
}