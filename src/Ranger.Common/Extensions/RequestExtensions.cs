using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Ranger.Common
{
    public static class RequestExtensions
    {
        public static string GetDomainFromHeader(this IHeaderDictionary headers)
        {
            string result = "";
            StringValues domain;
            bool success = headers.TryGetValue("X-Tenant-Domain", out domain);
            if (success)
            {
                if (domain.Count == 1)
                {
                    result = domain;
                }
                else
                {
                    throw new DomainNotFoundException("Multiple header values were found for X-Tenant-Domain.");
                }
            }
            return result;
        }

        public static User UserFromClaims(this ClaimsPrincipal user) => new User
            (
                user.Claims.SingleOrDefault(c => c.Type == "domain")?.Value ?? "",
                user.Claims.SingleOrDefault(c => c.Type == "email")?.Value ?? "",
                user.Claims.SingleOrDefault(c => c.Type == "firstName")?.Value ?? "",
                user.Claims.SingleOrDefault(c => c.Type == "lastName")?.Value ?? "",
                user.Claims.SingleOrDefault(c => c.Type == "phoneNumber")?.Value ?? "",
                SystemRole(user.Claims.Where(c => c.Type == "role").Select(c => c.Value))
            );

        private static string SystemRole(IEnumerable<string> roles)
        {
            var owner = Enum.GetName(typeof(RolesEnum), RolesEnum.Owner);
            var admin = Enum.GetName(typeof(RolesEnum), RolesEnum.Admin);
            var user = Enum.GetName(typeof(RolesEnum), RolesEnum.User);
            if (roles.Contains(owner))
            {
                return owner;
            }
            if (roles.Contains(admin))
            {
                return admin;
            }
            return user;
        }
    }
}