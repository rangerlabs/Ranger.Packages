using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface ISubscriptionsClient
    {
        Task<T> GenerateCheckoutExistingUrl<T>(string domain, string planId);
        Task<T> GetLimitDetails<T>(string domain);
        Task<T> GetSubscriptionPlanId<T>(string domain);
    }
}