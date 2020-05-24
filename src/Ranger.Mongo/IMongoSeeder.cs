using System.Threading.Tasks;

namespace Ranger.Mongo
{
    public interface IMongoDbSeeder
    {
        Task SeedAsync();
    }
}