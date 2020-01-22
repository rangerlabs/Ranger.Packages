using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IOperationsClient
    {
        Task<T> GetOperationStateById<T>(string domain, string id);
    }
}