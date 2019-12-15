using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IIdentityClient
    {
        Task<T> GetUserAsync<T>(string domain, string username);
        Task<T> GetAllUsersAsync<T>(string domain);
        Task<T> GetUserRoleAsync<T>(string domain, string email);
        Task<T> GetRoleAsync<T>(string name);
        Task<bool> ConfirmUserAsync(string domain, string userId, string jsonContent);
        Task<bool> UserConfirmPasswordResetAsync(string domain, string userId, string jsonContent);
        Task<bool> RequestPasswordReset(string domain, string email, string jsonContent);
        Task<bool> RequestEmailChange(string domain, string email, string jsonContent);
        Task UserConfirmEmailChangeAsync(string domain, string userId, string jsonContent);
    }
}