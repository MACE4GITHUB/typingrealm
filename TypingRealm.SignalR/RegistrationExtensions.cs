using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.SignalR
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection RegisterMessageHub(this IServiceCollection services)
        {
            services.AddSingleton<ISignalRServer, SignalRServer>();
            services.AddSignalRConnectionFactory();

            return services;
        }

        public static IServiceCollection AddSignalRConnectionFactory(this IServiceCollection services)
        {
            return services.AddTransient<ISignalRConnectionFactory, SignalRConnectionFactory>();
        }
    }
}
