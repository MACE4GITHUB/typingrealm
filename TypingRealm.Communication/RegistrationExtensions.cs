using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Communication
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddCommunication(this IServiceCollection services)
        {
            return services.AddTransient<IHttpClient, AnonymousHttpClient>();
        }
    }
}
