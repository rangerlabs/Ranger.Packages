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
            bool success = headers.TryGetValue("x-ranger-domain", out domain);
            if (success)
            {
                if (domain.Count == 1)
                {
                    result = domain;
                }
                else
                {
                    throw new DomainNotFoundException("Multiple header values were found for x-ranger-domain.");
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
                SystemRole(user.Claims.Where(c => c.Type == "role").Select(c => c.Value)),
                user.Claims.Where(c => c.Type == "authorizedProjects").Select(c => c.Value) ?? new string[0]
            );

        private static string SystemRole(IEnumerable<string> roles)
        {
            var PrimaryOwner = Enum.GetName(typeof(RolesEnum), RolesEnum.PrimaryOwner);
            var owner = Enum.GetName(typeof(RolesEnum), RolesEnum.Owner);
            var admin = Enum.GetName(typeof(RolesEnum), RolesEnum.Admin);
            var user = Enum.GetName(typeof(RolesEnum), RolesEnum.User);
            if (roles.Contains(PrimaryOwner))
            {
                return PrimaryOwner;
            }
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