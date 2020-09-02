using System.Threading;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IIdentityHttpClient
    {
        Task<RangerApiResponse> ConfirmUserAsync(string tenantId, string email, string jsonContent, CancellationToken cancellationToken = default);
        Task<RangerApiResponse<T>> GetAllUsersAsync<T>(string tenantId, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<T>> GetUserAsync<T>(string tenantId, string email, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<string>> GetUserRoleAsync(string tenantId, string email, CancellationToken cancellationToken = default);
        Task<RangerApiResponse> RequestEmailChange(string tenantId, string email, string jsonContent, CancellationToken cancellationToken = default);
        Task<RangerApiResponse> RequestPasswordReset(string tenantId, string email, string jsonContent, CancellationToken cancellationToken = default);
        Task<RangerApiResponse> UpdateUserOrAccountAsync(string tenantId, string email, string jsonContent, CancellationToken cancellationToken = default);
        Task<RangerApiResponse> UserConfirmEmailChangeAsync(string tenantId, string jsonContent, CancellationToken cancellationToken = default);
        Task<RangerApiResponse> UserConfirmPasswordResetAsync(string tenantId, string email, string jsonContent, CancellationToken cancellationToken = default);
    }
}