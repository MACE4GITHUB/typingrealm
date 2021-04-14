using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication.ConsoleClient.TokenProviders;
using TypingRealm.Messaging.Client;

namespace TypingRealm.Authentication.ConsoleClient
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddLocalProfileTokenProvider(this IServiceCollection services, string profile)
        {
            services.AddTransient<IProfileTokenProvider>(
                _ => new LocalProfileTokenProvider(profile));

            return services;
        }

        public static IServiceCollection AddAuth0ProfileTokenProvider(this IServiceCollection services)
        {
            var auth0Config = new Auth0AuthenticationConfiguration();
            services.AddSingleton<IProfileTokenProvider>(
                _ => new PkceProfileTokenProvider(auth0Config.Issuer, auth0Config.PkceClientId));

            return services;
        }

        public static IServiceCollection AddIdentityServerProfileTokenProvider(this IServiceCollection services)
        {
            var idsConfig = new IdentityServerAuthenticationConfiguration();
            services.AddSingleton<IProfileTokenProvider>(
                _ => new PkceProfileTokenProvider(idsConfig.Issuer, idsConfig.PkceClientId));

            return services;
        }
    }
}
