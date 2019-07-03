using System;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient {
    public interface IApiClient {
        Task SetClientToken ();
    }
}