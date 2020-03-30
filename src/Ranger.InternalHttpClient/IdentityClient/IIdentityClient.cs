using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IIdentityClient
    {
        Task DeleteAccountAsync(string domain, string email, string jsonContent);
        Task DeleteUserAsync(string domain, string email, string jsonContent);
        Task<T> GetUserAsync<T>(string domain, string username);
        Task<T> GetAllUsersAsync<T>(string domain);
        Task<T> GetUserRoleAsync<T>(string domain, string email);
        Task<bool> ConfirmUserAsync(string domain, string userId, string jsonContent);
        Task<bool> UserConfirmPasswordResetAsync(string domain, string userId, string jsonContent);
        Task<bool> RequestPasswordReset(string domain, string email, string jsonContent);
        Task<bool> RequestEmailChange(string domain, string email, string jsonContent);
        Task UpdateUserAsync(string domain, string username, string jsonContent);
        Task UserConfirmEmailChangeAsync(string domain, string userId, string jsonContent);
    }
}