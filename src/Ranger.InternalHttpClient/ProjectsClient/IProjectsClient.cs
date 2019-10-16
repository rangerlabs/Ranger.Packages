using System.Net.Http;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IProjectsClient
    {
        Task<T> GetAllProjectsAsync<T>(string domain);
        Task<T> PostProjectAsync<T>(HttpMethod method, string domain, string jsonContent);
        Task<T> PutProjectAsync<T>(HttpMethod method, string domain, string projectId, string jsonContent);
    }
}