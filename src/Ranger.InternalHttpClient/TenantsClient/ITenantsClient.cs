using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface ITenantsClient
    {
        Task<bool> ExistsAsync(string domain);
        Task<T> EnabledAsync<T>(string domain);
        Task<T> GetTenantAsync<T>(string domain);
        Task<bool> ConfirmTenantAsync(string domain, string jsonContent);
    }
}