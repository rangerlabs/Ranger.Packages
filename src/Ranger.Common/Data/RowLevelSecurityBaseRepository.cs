using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Ranger.Common;

namespace Ranger.Common
{
    public class RowLevelSecurityBaseRepository<TRepository, TReturnDbContext, TRowLevelSecurityDbContext>
        where TRepository : IRepository
        where TReturnDbContext : DbContext
        where TRowLevelSecurityDbContext : RowLevelSecurityDbContext<TReturnDbContext>
    {
        public delegate TRepository Factory(ContextTenant contextTenant);
        protected readonly TReturnDbContext Context;
        private readonly ILogger<RowLevelSecurityBaseRepository<TRepository, TReturnDbContext, TRowLevelSecurityDbContext>> logger;

        public RowLevelSecurityBaseRepository(ContextTenant contextTenant, RowLevelSecurityDbContext<TReturnDbContext>.Factory context, CloudSqlOptions cloudSqlOptions, ILogger<RowLevelSecurityBaseRepository<TRepository, TReturnDbContext, TRowLevelSecurityDbContext>> logger)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            if (contextTenant is null)
            {
                throw new System.ArgumentNullException(nameof(contextTenant));
            }

            if (cloudSqlOptions is null)
            {
                throw new System.ArgumentNullException(nameof(cloudSqlOptions));
            }

            NpgsqlConnectionStringBuilder connectionBuilder = new NpgsqlConnectionStringBuilder(cloudSqlOptions.ConnectionString);
            connectionBuilder.Username = contextTenant.DatabaseUsername;
            connectionBuilder.Password = contextTenant.DatabasePassword;

            var options = new DbContextOptionsBuilder<DbContext>();
            options.UseNpgsql(connectionBuilder.ToString());
            this.Context = context.Invoke(options.Options);
            this.logger = logger;
        }
    }
}