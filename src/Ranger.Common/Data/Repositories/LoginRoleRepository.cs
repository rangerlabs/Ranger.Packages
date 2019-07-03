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

            public async Task CreateTenantLoginRole (string username, string password) {
                using (var pgConnection = new NpgsqlConnection (context.Database.GetDbConnection ().ConnectionString))
                using (var command = new NpgsqlCommand (@"SELECT create_tenantloginrole(:username, :password);", pgConnection)) {
                    await pgConnection.OpenAsync ();
                    command.Parameters.Add (new NpgsqlParameter ("username", username));
                    command.Parameters.Add (new NpgsqlParameter ("password", password));
                    var result = await command.ExecuteScalarAsync ();
                    pgConnection.Close ();
                }
            }

            public async Task CreateTenantLoginRolePermissions (string username, string table) {
                using (var pgConnection = new NpgsqlConnection (context.Database.GetDbConnection ().ConnectionString))
                using (var command = new NpgsqlCommand (@"SELECT create_tenantloginrolepermissions(:username, :table);", pgConnection)) {
                    await pgConnection.OpenAsync ();
                    command.Parameters.Add (new NpgsqlParameter ("username", username));
                    command.Parameters.Add (new NpgsqlParameter ("table", table));
                    var result = await command.ExecuteScalarAsync ();
                    pgConnection.Close ();
                }
            }

            public async Task ApplyTenantLoginPolicy (string table) {
                using (var pgConnection = new NpgsqlConnection (context.Database.GetDbConnection ().ConnectionString))
                using (var command = new NpgsqlCommand (@"SELECT applytenantpolicy(:table);", pgConnection)) {
                    await pgConnection.OpenAsync ();
                    command.Parameters.Add (new NpgsqlParameter ("table", table));
                    var result = await command.ExecuteScalarAsync ();
                    pgConnection.Close ();
                }
            }
        }
}