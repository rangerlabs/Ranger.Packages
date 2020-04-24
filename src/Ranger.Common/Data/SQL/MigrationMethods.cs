using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ranger.Common
{
    public static class MultiTenantMigrationSql
    {
        public static string CreateTenantRlsPolicy()
        {
            return GetManifestResourceSql("create_tenant_rls_policy");
        }

        public static string CreateTenantLoginRole()
        {
            return GetManifestResourceSql("create_tenant_login_role");
        }

        public static string GrantTenantLoginRoleTablePermissions()
        {
            return GetManifestResourceSql("grant_tenant_login_role_table_permissions");
        }

        public static string GrantTenantLoginRoleSequencePermissions()
        {
            return GetManifestResourceSql("grant_tenant_login_role_sequence_permissions");
        }

        public static string RevokeTenantLoginRoleTablePermissions()
        {
            return GetManifestResourceSql("revoke_tenant_login_role_table_permissions");
        }

        public static string RevokeTenantLoginRoleSequencePermissions()
        {
            return GetManifestResourceSql("revoke_tenant_login_role_sequence_permissions");
        }

        public static string DropTenantLoginRole()
        {
            return GetManifestResourceSql("drop_tenant_login_role");
        }

        private static string GetManifestResourceSql(string name)
        {
            string sql;
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (var manifestStream = assembly.GetManifestResourceStream($"Ranger.Common.Data.SQL.{name}.sql"))
            {
                if (manifestStream is null)
                {
                    throw new Exception($@"Failed to file ""Ranger.Common.Data.SQL.{name}.sql"" in manifest files {String.Join(";", assembly.GetManifestResourceNames())} ");
                }
                using (var reader = new StreamReader(manifestStream))
                {
                    sql = reader.ReadToEnd();
                }
                if (String.IsNullOrWhiteSpace(sql))
                {
                    throw new Exception($"The file '{name}.sql' was empty");
                }
            }
            return sql;
        }
    }
}