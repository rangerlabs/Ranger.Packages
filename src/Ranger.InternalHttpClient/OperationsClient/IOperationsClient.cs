using System;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IOperationsClient
    {
        Task<T> GetOperationStateById<T>(string domain, Guid id);
    }
}