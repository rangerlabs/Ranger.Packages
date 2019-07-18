using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Ranger.Common {
    public static class RequestExtensions {
        public static string GetDomainFromHeader (this IHeaderDictionary headers) {
            string result = "";
            StringValues domain;
            bool success = headers.TryGetValue ("X-Tenant-Domain", out domain);
            if (success) {
                if (domain.Count == 1) {
                    result = domain;
                } else {
                    throw new DomainNotFoundException ("Multiple header values were found for X-Tenant-Domain.");
                }
            }
            return result;
        }
    }
}