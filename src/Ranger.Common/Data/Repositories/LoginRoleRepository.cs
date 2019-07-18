using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Ranger.Common {
    public class LoginRoleRepository<TContext> : ILoginRoleRepository<TContext>
        where TContext : DbContext {
            private readonly TContext context;

            public LoginRoleRepository (TContext context) {
                this.context = context;
            }

            public async Task<int> CreateTenantLoginRole (string username, string password) {
                return await context.Database.ExecuteSqlCommandAsync ($@"SELECT create_tenantloginrole({username}, {password});");
            }

            public async Task<int> CreateTenantLoginRolePermissions (string username, string table) {
                return await context.Database.ExecuteSqlCommandAsync ($@"SELECT create_tenantloginrolepermissions({username}, {table});");
            }

            public async Task<int> CreateTenantLoginPolicy (string table) {
                return await context.Database.ExecuteSqlCommandAsync ($@"SELECT create_tenantpolicy({table});");
            }
        }
}