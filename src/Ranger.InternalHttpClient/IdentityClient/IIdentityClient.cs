using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient {
    public interface IIdentityClient : IApiClient {

        Task<InternalApiResponse<T>> GetUserAsync<T> (string username);
        Task<InternalApiResponse<T>> GetAllUsersAsync<T> ();
        Task<InternalApiResponse> CreateUser (ApplicationUserRequestModel user);
        Task<InternalApiResponse<T>> GetRoleAsync<T> (string name);
    }
}