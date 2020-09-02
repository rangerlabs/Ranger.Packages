using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IIntegrationsHttpClient
    {
        Task<RangerApiResponse<int>> GetAllActiveIntegrationsCount(string tenantId, CancellationToken cancellationToken = default);
        Task<RangerApiResponse<T>> GetAllIntegrationsByProjectId<T>(string tenantId, Guid projectId, CancellationToken cancellationToken = default)
            where T : class;
    }
}