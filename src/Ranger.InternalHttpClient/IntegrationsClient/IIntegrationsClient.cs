using System;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IIntegrationsClient
    {
        Task<T> GetAllIntegrationsByProjectId<T>(string domain, Guid projectId);
    }
}