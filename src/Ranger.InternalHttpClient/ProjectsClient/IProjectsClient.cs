using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface IProjectsClient
    {
        Task<T> PostProjectAsync<T>(string domain, string jsonContent);
    }
}