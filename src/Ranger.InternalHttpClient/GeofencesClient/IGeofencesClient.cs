using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IGeofencesClient
    {
        Task<T> GetAllGeofencesByProjectId<T>(string domain, string projectId);
    }
}