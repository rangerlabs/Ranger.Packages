using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient {
    public interface IIdentityClient {

        Task<InternalApiResponse<T>> GetUserAsync<T> (string domain, string username);
        Task<InternalApiResponse<T>> GetAllUsersAsync<T> (string domain);
        Task<InternalApiResponse<T>> GetRoleAsync<T> (string name);
    }
}