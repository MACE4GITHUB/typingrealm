using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication.OAuth;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.Authentication
{
    public static class CommonRegistrationExtensions
    {
        /// <summary>
        /// Shared code that is run both for API and Services.
        /// </summary>
        public static IAuthenticationInformationProvider AddTyrCommonAuthentication(this IServiceCollection services)
        {
            var profileAuthentication = BuildAuth0OrLocal();

            var serviceAuthentication = new AuthenticationInformationBuilder()
                .UseIdentityServerProvider()
                .Build();

            return services.AddTyrCommonAuthentication(profileAuthentication, serviceAuthentication);
        }

        /// <summary>
        /// Common dependencies used both by Services and APIs.
        /// </summary>
        private static IAuthenticationInformationProvider AddTyrCommonAuthentication(
            this IServiceCollection services,
            AuthenticationInformation profileAuthentication,
            AuthenticationInformation? serviceAuthentication = null)
        {
            // S2S token issuer.
            services.AddTransient<IServiceTokenService, ServiceTokenService>();

            // Adds possibility to query Profiles API (to get Profile data).
            services.AddProfileApiClients();

            // Registers authentication data that is used by S2S token issuer and validation.
            var authenticationInformationProvider = new AuthenticationInformationProvider(
                profileAuthentication, serviceAuthentication ?? profileAuthentication);
            services.AddSingleton<IAuthenticationInformationProvider>(authenticationInformationProvider);

            // Validates token and creates Principal and Security objects.
            // Previously worked only in messaging, now registered here. Useful thing to have.
            services.AddTransient<ITokenAuthenticationService, TokenAuthenticationService>();

            return authenticationInformationProvider;
        }

        private static AuthenticationInformation BuildAuth0OrLocal()
        {
            if (DebugHelpers.UseLocalAuthentication)
            {
                return new AuthenticationInformationBuilder()
                    .UseLocalProvider()
                    .Build();
            }

            return new AuthenticationInformationBuilder()
                .UseAuth0Provider()
                .Build();
        }
    }
}
