using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging.Serialization;

namespace TypingRealm.Authentication
{
    public static class RegistrationExtensions
    {
        public static TyrAuthenticationBuilder AddTypingRealmAuthentication(this IServiceCollection services)
        {
            return new TyrAuthenticationBuilder(services);
        }

        public static TyrAuthenticationBuilder AddTypingRealmAuthentication(this MessageTypeCacheBuilder builder)
        {
            builder.AddTyrAuthenticationMessages();

            return builder.Services.AddTypingRealmAuthentication();
        }

        public static TyrAuthenticationBuilder AddTyrApiAuthentication(this IServiceCollection services)
        {
            return services.AddTypingRealmAuthentication()
                //.UseAuth0Provider()
                .UseIdentityServerProvider()
                .UseAspNetAuthentication();
        }

        public static TyrAuthenticationBuilder AddTyrWebServiceAuthentication(this MessageTypeCacheBuilder builder)
        {
            return builder.AddTypingRealmAuthentication()
                //.UseAuth0Provider()
                .UseIdentityServerProvider()
                .UseAspNetAuthentication()
                .UseConnectedClientContextAuthentication()
                .UseCharacterAuthorizationOnConnect();
        }

        // TODO: Move it to separate assembly, not related to AspNet.
        public static TyrAuthenticationBuilder AddTyrServiceWithoutAspNetAuthentication(this MessageTypeCacheBuilder builder)
        {
            return builder.AddTypingRealmAuthentication()
                //.UseAuth0Provider()
                .UseIdentityServerProvider()
                .UseConnectedClientContextAuthentication()
                .UseCharacterAuthorizationOnConnect();
        }

        public static MessageTypeCacheBuilder AddTyrAuthenticationMessages(this MessageTypeCacheBuilder builder)
        {
            return builder.AddMessageTypesFromAssembly(typeof(Authenticate).Assembly);
        }
    }
}
