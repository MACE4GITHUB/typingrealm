using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Data.Infrastructure.DataAccess;
using TypingRealm.Data.Infrastructure.DataAccess.Repositories;
using TypingRealm.Typing;

namespace TypingRealm.Data.Infrastructure
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection RegisterDataApi(this IServiceCollection services, string dataConnectionString)
        {
            // Typing Domain.
            services.AddTyping();

            services.AddSingleton<ILocationRepository, InMemoryLocationRepository>();

            // Typing.
            services.AddSingleton<ITextGenerator, TextGenerator>();

            // Repositories.
            if (DebugHelpers.UseInfrastructure)
            {
                services.AddTransient<ITextRepository, TextRepository>();
                services.AddTransient<ITypingSessionRepository, TypingSessionRepository>();
                services.AddTransient<IUserSessionRepository, UserSessionRepository>();

                services.AddDbContext<DataContext>(
                    options => options
                        .UseNpgsql(dataConnectionString)
                        .UseSnakeCaseNamingConvention());

                services.AddTransient<IInfrastructureDeploymentService, InfrastructureDeploymentService>();
                services.AddTransient<IUserTypingStatisticsStore, UserTypingStatisticsStore>();
            }
            else
            {
                services.AddTransient<IInfrastructureDeploymentService, NoInfrastructureService>();
            }

            return services;
        }
    }
}
