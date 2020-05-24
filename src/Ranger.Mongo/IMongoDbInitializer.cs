using System.Threading.Tasks;

namespace Ranger.Mongo
{
    public interface IMongoDbInitializer
    {
        Task Initialize();
    }
}