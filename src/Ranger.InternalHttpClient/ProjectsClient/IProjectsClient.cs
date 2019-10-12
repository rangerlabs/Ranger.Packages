using System.Net.Http;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IProjectsClient
    {
        Task<T> GetAllProjectsAsync<T>(string domain);
        Task<T> SendProjectAsync<T>(HttpMethod method, string domain, string jsonContent);
    }
}