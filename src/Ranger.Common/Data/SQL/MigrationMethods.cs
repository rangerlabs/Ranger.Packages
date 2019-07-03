using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ranger.Common {
    public static class MultiTenantMigrationMethods {
        public static string CreateTenantLoginRole () {
            string sql;
            Assembly assembly = Assembly.GetExecutingAssembly ();
            using (var manifestStream = assembly.GetManifestResourceStream ("Ranger.Common.Data.SQL.create_tenantloginrole.sql")) {
                if (manifestStream is null) {
                    throw new RangerException ($@"Failed to file ""Ranger.Common.Data.SQL.create_tenantloginrole.sql"" in manifest files {String.Join(";", assembly.GetManifestResourceNames())} ");
                }
                using (var reader = new StreamReader (manifestStream)) {
                    sql = reader.ReadToEnd ();
                }
                if (String.IsNullOrWhiteSpace (sql)) {
                    throw new Exception ("The file 'create_tenantloginrole.sql' was empty.");
                }
            }
            return sql;
        }
        public static string CreateTenantLoginRolePermissions () {
            string sql;
            Assembly assembly = Assembly.GetExecutingAssembly ();
            using (var manifestStream = assembly.GetManifestResourceStream ("Ranger.Common.Data.SQL.create_tenantloginrolepermissions.sql")) {
                if (manifestStream is null) {
                    throw new RangerException ($@"Failed to file ""Ranger.Common.Data.SQL.create_tenantloginrolepermissions.sql"" in manifest files {String.Join(";", assembly.GetManifestResourceNames())} ");
                }
                using (var reader = new StreamReader (manifestStream)) {
                    sql = reader.ReadToEnd ();
                }
                if (String.IsNullOrWhiteSpace (sql)) {
                    throw new Exception ("The file 'create_tenantloginrolepermissions.sql' was empty.");
                }
            }
            return sql;
        }

        public static string CreateTenantPolicy () {
            string sql;
            Assembly assembly = Assembly.GetExecutingAssembly ();
            Console.WriteLine (assembly.GetManifestResourceNames ());
            using (var manifestStream = assembly.GetManifestResourceStream ("Ranger.Common.Data.SQL.create_tenantpolicy.sql")) {
                if (manifestStream is null) {
                    throw new RangerException ($@"Failed to file ""Ranger.Common.Data.SQL.create_tenantpolicy.sql"" in manifest files {String.Join(";", assembly.GetManifestResourceNames())} ");
                }
                using (var reader = new StreamReader (manifestStream)) {
                    sql = reader.ReadToEnd ();
                }
                if (String.IsNullOrWhiteSpace (sql)) {
                    throw new Exception ("The file 'create_tenantpolicy.sql' was empty.");
                }
            }
            return sql;
        }
    }
}