using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging.Client;

namespace TypingRealm.SignalR.Client
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection UseSignalRClientConnectionFactory(
            this IServiceCollection services, string uri)
        {
            services.AddTransient<IClientConnectionFactory>(provider =>
            {
                var factory = provider.GetRequiredService<ISignalRConnectionFactory>();
                var profileTokenProvider = provider.GetRequiredService<IProfileTokenProvider>();

                return new SignalRClientConnectionFactory(factory, profileTokenProvider, uri);
            });

            return services;
        }
    }
}
