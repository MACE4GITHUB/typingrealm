using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Authentication
{
    public static class RegistrationExtensions
    {
        public static TyrAuthenticationBuilder AddTypingRealmAuthentication(this IServiceCollection services)
        {
            return new TyrAuthenticationBuilder(services);
        }

        public static TyrAuthenticationBuilder AddTyrApiAuthentication(this IServiceCollection services)
        {
            return services.AddTypingRealmAuthentication()
                .UseAuth0Provider()
                .UseAspNetAuthentication();
        }

        public static TyrAuthenticationBuilder AddTyrWebServiceAuthentication(this IServiceCollection services)
        {
            return services.AddTypingRealmAuthentication()
                .UseAuth0Provider()
                .UseAspNetAuthentication()
                .UseConnectMessageAuthentication()
                .UseCharacterAuthorizationOnConnect();
        }

        public static TyrAuthenticationBuilder AddTyrServiceWithoutAspNetAuthentication(this IServiceCollection services)
        {
            return services.AddTypingRealmAuthentication()
                .UseAuth0Provider()
                .UseConnectMessageAuthentication()
                .UseCharacterAuthorizationOnConnect();
        }
    }
}
