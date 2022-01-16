using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Hosting.HealthChecks
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddTyrHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks();
            services.AddHostedService<LogHealthChecksHostedService>();

            return services;
        }
    }
}
