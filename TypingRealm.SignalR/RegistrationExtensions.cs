using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.SignalR
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection RegisterMessageHub(this IServiceCollection services)
        {
            return services.AddSingleton<ConcurrentDictionary<string, SignalRConnectionResource>>();
        }
    }
}
