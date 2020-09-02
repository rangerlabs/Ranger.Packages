using System;
using System.Threading;
using System.Threading.Tasks;
using Ranger.Common;

namespace Ranger.InternalHttpClient
{
    public interface IProjectsHttpClient
    {
        Task<RangerApiResponse<T>> ApiKeyResetAsync<T>(string tenantId, Guid projectId, ApiKeyPurposeEnum purpose, string jsonContent, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<T>> GetAllProjects<T>(string tenantId, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<T>> GetAllProjectsForUserAsync<T>(string tenantId, string email, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<T>> GetProjectByApiKeyAsync<T>(string tenantId, string apiKey, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<T>> GetProjectByNameAsync<T>(string tenantId, string projectName, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<T>> PostProjectAsync<T>(string tenantId, string jsonContent, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<T>> PutProjectAsync<T>(string tenantId, Guid projectId, string jsonContent, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse> SoftDeleteProjectAsync(string tenantId, Guid projectId, string userEmail, CancellationToken cancellationToken = default);
        Task<RangerApiResponse<string>> GetTenantIdByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
    }
}