using System.Threading.Tasks;

namespace Ranger.InternalHttpClient {
    public interface ITenantsClient {
        Task<InternalApiResponse> ExistsAsync (string domain);
        Task<InternalApiResponse<T>> GetTenantAsync<T> (string domain);
    }
}