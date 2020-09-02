using System.Threading;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface ITenantsHttpClient
    {
        Task<RangerApiResponse> ConfirmTenantAsync(string domain, string jsonContent, CancellationToken cancellationToken = default);
        Task<RangerApiResponse<bool>> DoesExistAsync(string domain, CancellationToken cancellationToken = default);
        Task<RangerApiResponse<T>> GetAllTenantsAsync<T>(CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<T>> GetPrimaryOwnerTransferByDomain<T>(string domain, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<T>> GetTenantByDomainAsync<T>(string domain, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<T>> GetTenantByIdAsync<T>(string tenantId, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<bool>> IsConfirmedAsync(string domain, CancellationToken cancellationToken = default);
    }
}