using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ranger.Common
{
    public interface ILoginRoleRepository<in TContext> where TContext : DbContext
    {

        Task<int> CreateTenantLoginRole(string username, string password);
        Task<int> DropTenantLoginRole(string username);
        Task<int> RevokeTenantLoginRoleTablePermissions(string username, string table);
        Task<int> GrantTenantLoginRoleTablePermissions(string username, string table);
        Task<int> RevokeTenantLoginRoleSequencePermissions(string username);
        Task<int> GrantTenantLoginRoleSequencePermissions(string username);
        Task<int> CreateTenantRlsPolicy(string table);
    }
}