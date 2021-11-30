using TypingRealm.Authentication.Api;
using TypingRealm.Authentication.Service;
using TypingRealm.Messaging.Serialization;

namespace TypingRealm.Authentication
{
    public static class RegistrationExtensions
    {
        public static MessageTypeCacheBuilder AddTyrServiceAuthentication(this MessageTypeCacheBuilder builder)
        {
            var services = builder.Services;
            var authInfoProvider = services.AddTyrCommonAuthentication();
            services.UseAspNetAuthentication(
                authInfoProvider.GetProfileAuthenticationInformation(),
                authInfoProvider.GetServiceAuthenticationInformation());

            builder.AddMessagingServiceAuthentication();

            return builder;
        }
    }
}
