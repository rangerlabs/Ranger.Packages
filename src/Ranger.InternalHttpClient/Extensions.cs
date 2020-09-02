using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;

namespace Ranger.InternalHttpClient
{
    public static class Extensions
    {
        public static IServiceCollection AddTenantsHttpClient(this IServiceCollection services, string baseAddress, string identityAuthority, string scope, string clientSecret)
            => AddHttpClient<ITenantsHttpClient, TenantsHttpClient>(services, "TenantsHttpClient", baseAddress, identityAuthority, scope, clientSecret);
        public static IServiceCollection AddProjectsHttpClient(this IServiceCollection services, string baseAddress, string identityAuthority, string scope, string clientSecret)
            => AddHttpClient<IProjectsHttpClient, ProjectsHttpClient>(services, "ProjectsHttpClient", baseAddress, identityAuthority, scope, clientSecret);
        public static IServiceCollection AddIdentityHttpClient(this IServiceCollection services, string baseAddress, string identityAuthority, string scope, string clientSecret)
            => AddHttpClient<IIdentityHttpClient, IdentityHttpClient>(services, "IdentityHttpClient", baseAddress, identityAuthority, scope, clientSecret);
        public static IServiceCollection AddSubscriptionsHttpClient(this IServiceCollection services, string baseAddress, string identityAuthority, string scope, string clientSecret)
            => AddHttpClient<ISubscriptionsHttpClient, SubscriptionsHttpClient>(services, "SubscriptionsHttpClient", baseAddress, identityAuthority, scope, clientSecret);
        public static IServiceCollection AddGeofencesHttpClient(this IServiceCollection services, string baseAddress, string identityAuthority, string scope, string clientSecret)
            => AddHttpClient<IGeofencesHttpClient, GeofencesHttpClient>(services, "GeofencesHttpClient", baseAddress, identityAuthority, scope, clientSecret);
        public static IServiceCollection AddIntegrationsHttpClient(this IServiceCollection services, string baseAddress, string identityAuthority, string scope, string clientSecret)
            => AddHttpClient<IIntegrationsHttpClient, IntegrationsHttpClient>(services, "IntegrationsHttpClient", baseAddress, identityAuthority, scope, clientSecret);

        public static IServiceCollection AddPollyPolicyRegistry(this IServiceCollection services)
        {
            var registry = new PolicyRegistry()
            {
                {
                    "AuthorizationRetryPolicy",
                    PollyPolicies.AuthorizationRetryPolicy
                },
                {
                    "ExponentialBackoffWithJitterPolicy",
                    PollyPolicies.ExponentialBackoffWithJitterPolicy()
                },
                {
                    "NoOpPolicy",
                    PollyPolicies.NoOpPolicy
                }
            };
            services.AddPolicyRegistry(registry);
            return services;
        }


        static IServiceCollection AddHttpClient<TInterface, TConcrete>(IServiceCollection services, string clientId, string baseAddress, string identityAuthority, string scope, string clientSecret)
            where TInterface : class
            where TConcrete : ApiClientBase, TInterface
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new System.ArgumentException($"{nameof(clientId)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(baseAddress))
            {
                throw new System.ArgumentException($"{nameof(baseAddress)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(identityAuthority))
            {
                throw new System.ArgumentException($"{nameof(identityAuthority)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new System.ArgumentException($"{nameof(scope)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new System.ArgumentException($"{nameof(clientSecret)} was null or whitespace");
            }
            services.AddSingleton(new HttpClientOptions<TConcrete>(baseAddress, identityAuthority, scope, clientId, clientSecret));
            services.AddHttpClient<TInterface, TConcrete>(clientId)
                .SetHandlerLifetime(TimeSpan.FromMinutes(30)) //same lifetime of access tokens
                .AddPolicyHandlerFromRegistry("AuthorizationRetryPolicy")
                .AddPolicyHandlerFromRegistry((a, r) =>
                    r.Method == HttpMethod.Get ?
                    a.Get<IAsyncPolicy<HttpResponseMessage>>("ExponentialBackoffWithJitterPolicy") :
                    a.Get<IAsyncPolicy<HttpResponseMessage>>("NoOpPolicy"));
            return services;
        }
    }
}