using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ranger.Common;

namespace Ranger.InternalHttpClient
{
    public interface IGeofencesHttpClient
    {
        Task<RangerApiResponse<long>> GetAllActiveGeofencesCount(string tenantId, CancellationToken cancellationToken = default);
        Task<RangerApiResponse<T>> GetGeofencesByProjectId<T>(string tenantId, Guid projectId, string orderBy, string sortOrder, int page, int pageCount, CancellationToken cancellationToken = default(CancellationToken))
            where T : class;
        Task<RangerApiResponse<T>> GetGeofencesByBounds<T>(string tenantId, Guid projectId, string orderBy, string sortOrder, IEnumerable<LngLat> bounds, CancellationToken cancellationToken = default(CancellationToken))
            where T : class;
    }
}