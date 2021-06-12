using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TypingRealm.Communication
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddCommunication(this IServiceCollection services)
        {
            services.TryAddTransient<IAccessTokenProvider, AnonymousAccessTokenProvider>();

            // Register as scoped so that we have one http client per connection.
            services.AddScoped<IHttpClient, HttpClient>();

            services.AddTransient<IServiceClient, InMemoryServiceClient>();

            return services;
        }
    }
}
