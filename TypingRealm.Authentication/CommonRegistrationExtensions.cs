using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Authentication.OAuth;

namespace TypingRealm.Authentication;

public static class CommonRegistrationExtensions
{
    /// <summary>
    /// Shared code that is run both for API and Services.
    /// </summary>
    public static IAuthenticationInformationProvider AddTyrCommonAuthentication(this IServiceCollection services)
    {
        var authentication = BuildAuthenticationInformationProvider();

        return services.AddTyrCommonAuthentication(authentication);
    }

    /// <summary>
    /// Common dependencies used both by Services and APIs.
    /// </summary>
    private static IAuthenticationInformationProvider AddTyrCommonAuthentication(
        this IServiceCollection services,
        AuthenticationInformationProvider authenticationInformationProvider)
    {
        // S2S token issuer.
        services.AddTransient<IServiceTokenService, ServiceTokenService>();

        // Registers authentication data that is used by S2S token issuer and validation.
        services.AddSingleton<IAuthenticationInformationProvider>(authenticationInformationProvider);

        // Validates token and creates Principal and Security objects.
        // Previously worked only in messaging, now registered here. Useful thing to have.
        services.AddTransient<ITokenAuthenticationService, TokenAuthenticationService>();

        return authenticationInformationProvider;
    }

    private static AuthenticationInformationProvider BuildAuthenticationInformationProvider()
    {
        var serviceAuthentication = new AuthenticationInformationBuilder()
            .UseIdentityServerProvider()
            .Build(TyrAuthenticationSchemes.ServiceAuthenticationScheme);

        if (DebugHelpers.UseDevelopmentAuthentication)
        {
            var profileAuthentication = new AuthenticationInformationBuilder()
                .UseAuth0Provider(isDevelopment: true)
                .Build(TyrAuthenticationSchemes.ProfileAuthenticationScheme);

            var localProfileAuthentication = new AuthenticationInformationBuilder()
                .UseLocalProvider()
                .Build(TyrAuthenticationSchemes.LocalProfileAuthenticationScheme);

            return new AuthenticationInformationProvider(
                localProfileAuthentication,
                new[] { profileAuthentication },
                serviceAuthentication);
        }

        {
            var profileAuthentication = new AuthenticationInformationBuilder()
                .UseAuth0Provider(isDevelopment: false)
                .Build(TyrAuthenticationSchemes.ProfileAuthenticationScheme);

            return new AuthenticationInformationProvider(
                profileAuthentication, serviceAuthentication);
        }
    }
}
