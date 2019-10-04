using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Ranger.Common
{
    public class RowLevelSecurityDbContext<TReturnDbContext> : DbContext
        where TReturnDbContext : DbContext
    {
        public RowLevelSecurityDbContext(DbContextOptions options) : base(options)
        {
            if (options is null)
            {
                throw new System.ArgumentNullException(nameof(options));
            }
        }

        protected RowLevelSecurityDbContext() : base()
        { }

        //Autofac Factory delegate
        public delegate TReturnDbContext Factory(DbContextOptions<DbContext> options);
    }
}