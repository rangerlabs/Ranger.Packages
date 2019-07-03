using System.Threading.Tasks;

namespace Ranger.InternalHttpClient {
    public interface ITenantsClient : IApiClient {
        Task<InternalApiResponse<bool>> ExistsAsync (string domain);
        Task<InternalApiResponse<T>> GetTenantAsync<T> (string domain);
    }
}