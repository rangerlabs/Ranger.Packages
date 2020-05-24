using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Ranger.Common
{
    public class LoginRoleRepository<TContext> : ILoginRoleRepository<TContext>
        where TContext : DbContext
    {
        private readonly TContext context;

        public LoginRoleRepository(TContext context)
        {
            this.context = context;
        }

        public async Task<int> CreateTenantLoginRole(string username, string password)
        {
            return await context.Database.ExecuteSqlInterpolatedAsync($@"SELECT create_tenant_login_role({username}, {password});");
        }
        public async Task<int> DropTenantLoginRole(string username)
        {
            return await context.Database.ExecuteSqlInterpolatedAsync($@"SELECT drop_tenant_login_role({username});");
        }

        public async Task<int> RevokeTenantLoginRoleTablePermissions(string username, string table)
        {
            return await context.Database.ExecuteSqlInterpolatedAsync($@"SELECT revoke_tenant_login_role_table_permissions({username}, {table});");
        }

        public async Task<int> GrantTenantLoginRoleTablePermissions(string username, string table)
        {
            return await context.Database.ExecuteSqlInterpolatedAsync($@"SELECT grant_tenant_login_role_table_permissions({username}, {table});");
        }

        public async Task<int> RevokeTenantLoginRoleSequencePermissions(string username)
        {
            return await context.Database.ExecuteSqlInterpolatedAsync($@"SELECT revoke_tenant_login_role_sequence_permissions({username});");
        }

        public async Task<int> GrantTenantLoginRoleSequencePermissions(string username)
        {
            return await context.Database.ExecuteSqlInterpolatedAsync($@"SELECT grant_tenant_login_role_sequence_permissions({username});");
        }

        public async Task<int> CreateTenantRlsPolicy(string table)
        {
            return await context.Database.ExecuteSqlInterpolatedAsync($@"SELECT create_tenant_rls_policy({table});");
        }
    }
}