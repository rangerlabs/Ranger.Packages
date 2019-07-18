using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ranger.Common {
    public interface ILoginRoleRepository<in TContext> where TContext : DbContext {

        Task<int> CreateTenantLoginRole (string username, string password);
        Task<int> CreateTenantLoginRolePermissions (string username, string table);
        Task<int> CreateTenantLoginPolicy (string table);
    }
}