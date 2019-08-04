using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient {
    public interface IIdentityClient {

        Task<T> GetUserAsync<T> (string domain, string username);
        Task<T> GetAllUsersAsync<T> (string domain);
        Task<T> GetRoleAsync<T> (string name);
    }
}