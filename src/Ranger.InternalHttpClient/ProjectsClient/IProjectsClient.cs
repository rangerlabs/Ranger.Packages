using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IProjectsClient
    {
        Task<IEnumerable<string>> GetProjectIdsForUser(string domain, string email);
        Task<T> GetDatabaseUsernameByApiKeyAsync<T>(string apiKey);
        Task<T> GetProjectByApiKeyAsync<T>(string domain, string apiKey);
        Task<T> GetAllProjectsForUserAsync<T>(string domain, string user);
        Task<T> PostProjectAsync<T>(string domain, string jsonContent);
        Task<T> PutProjectAsync<T>(string domain, string projectId, string jsonContent);
        Task<T> ApiKeyResetAsync<T>(string domain, string projectId, string environment, string jsonContent);
        Task SoftDeleteProjectAsync(string domain, string projectId, string userEmail);
    }
}