using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ranger.Common {
    public interface ILoginRoleRepository<in TContext> where TContext : DbContext {

        Task CreateTenantLoginRole (string username, string password);
        Task CreateTenantLoginRolePermissions (string username, string table);
        Task ApplyTenantLoginPolicy (string table);
    }
}