using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IOperationsClient
    {
        Task<RangerApiResponse<T>> GetOperationStateById<T>(string domain, Guid id, CancellationToken cancellationToken = default)
            where T : class;
    }
}