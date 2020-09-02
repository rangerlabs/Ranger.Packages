using System.Threading;
using System.Threading.Tasks;

namespace Ranger.InternalHttpClient
{
    public interface ISubscriptionsHttpClient
    {
        Task<RangerApiResponse<T>> GenerateCheckoutExistingUrl<T>(string tenantId, string planId, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<T>> GetPortalSession<T>(string tenantId, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<T>> GetSubscription<T>(string tenantId, CancellationToken cancellationToken = default)
            where T : class;
        Task<RangerApiResponse<string>> GetSubscriptionPlanId(string tenantId, CancellationToken cancellationToken = default);
        Task<RangerApiResponse<string>> GetTenantIdForSubscriptionId(string subscriptionId, CancellationToken cancellationToken = default);
        Task<RangerApiResponse<bool>> IsSubscriptionActive(string tenantId, CancellationToken cancellationToken = default);
    }
}