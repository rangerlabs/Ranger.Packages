using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Ranger.Common
{
    public static class Extensions
    {
        public static string Underscore(this string value) => string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()));

        public static TModel GetOptions<TModel>(this IConfiguration configuration, string section) where TModel : new()
        {
            if (String.IsNullOrWhiteSpace(section))
            {
                throw new ArgumentNullException(section);
            }
            var model = new TModel();
            configuration.GetSection(section).Bind(model);

            return model;
        }

        public static bool IsIntegrationTesting(this IConfiguration configuration)
        {
            var integrationTestingValue = configuration["INTEGRATION_TESTING"];
            return (!(integrationTestingValue is null) && Boolean.Parse(integrationTestingValue.ToLowerInvariant())) ? true : false;
        }
    }
}