using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IProjectsClient
    {
        Task<T> GetAllProjectsAsycn<T>(string domain);
        Task<T> PostProjectAsync<T>(string domain, string jsonContent);
    }
}