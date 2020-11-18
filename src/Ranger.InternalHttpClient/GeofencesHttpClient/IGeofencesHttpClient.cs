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
        Task<RangerApiResponse<T>> GetGeofencesByProjectId<T>(string tenantId, Guid projectId, string orderBy, string sortOrder, int page, int pageCount, string search, CancellationToken cancellationToken = default(CancellationToken))
            where T : class;
        Task<RangerApiResponse<T>> GetGeofencesByBounds<T>(string tenantId, Guid projectId, string orderBy, string sortOrder, IEnumerable<LngLat> bounds, CancellationToken cancellationToken = default(CancellationToken))
            where T : class;
        Task<RangerApiResponse<T>> GetGeofenceByExternalId<T>(string tenantId, Guid projectId, string externalId, CancellationToken cancellationToken = default(CancellationToken))
            where T : class;
        Task<RangerApiResponse<long>> GetGeofencesCountForProject(string tenantId, Guid projectId, CancellationToken cancellationToken = default(CancellationToken));
    }
}