using System;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IGeofencesClient
    {
        Task<T> GetAllGeofencesByProjectId<T>(string domain, Guid projectId);
    }
}