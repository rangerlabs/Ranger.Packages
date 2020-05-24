using System.Threading.Tasks;
using Ranger.Common;

namespace Ranger.InternalHttpClient
{
    public interface ISubscriptionsClient
    {
        Task<T> GenerateCheckoutExistingUrl<T>(string domain, string planId);
        Task<T> GetLimitDetails<T>(string domain);
        Task<T> GetSubscriptionPlanId<T>(string domain);
        Task<T> DecrementResource<T>(string domain, ResourceEnum resource);
        Task<T> IncrementResource<T>(string domain, ResourceEnum resource);
    }
}