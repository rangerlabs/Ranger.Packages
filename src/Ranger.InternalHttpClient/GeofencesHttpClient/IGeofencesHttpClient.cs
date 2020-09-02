using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IGeofencesHttpClient
    {
        Task<RangerApiResponse<long>> GetAllActiveGeofencesCount(string tenantId, CancellationToken cancellationToken = default);
        Task<RangerApiResponse<T>> GetAllGeofencesByProjectId<T>(string tenantId, Guid projectId, CancellationToken cancellationToken = default)
            where T : class;
    }
}