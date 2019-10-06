using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Ranger.Common
{
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
        public static string GetDomainFromHost(this HostString host)
        {
            List<string> fullAddress = host.ToString().Split('.').ToList();
            if (fullAddress.Count < 3)
                throw new DomainNotFoundException("No domain was found in the request.");
            return fullAddress[0];
        }

        public static string[] GetCascadedRoles(this RolesEnum role)
        {
            var roleValues = Enum.GetValues(typeof(RolesEnum));
            var cascadedRoles = new List<string>();
            for (int i = (int)role; i < roleValues.Length; i++)
            {
                cascadedRoles.Add(Enum.GetName(typeof(RolesEnum), i));
            }

            return cascadedRoles.ToArray();
        }
    }
}