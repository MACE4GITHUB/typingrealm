using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TypingRealm.Data.Infrastructure.DataAccess;
using TypingRealm.Data.Infrastructure.DataAccess.Repositories;
using TypingRealm.Typing;

namespace TypingRealm.Data.Infrastructure
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection RegisterDataApi(
            this IServiceCollection services,
            string dataConnectionString,
            string cacheConnectionString,
            string dataCacheConnectionString)
        {
            // Typing Domain.
            services.AddTyping();

            services.AddSingleton<ILocationRepository, InMemoryLocationRepository>();

            // Typing.
            services.AddSingleton<ITextGenerator, TextGenerator>();

            // TODO: Register this as transient and decorate as singleton, allow changing lifetime when decorating.
            services.AddSingleton<ITextRetriever, QuotableTextRetriever>();

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

                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = cacheConnectionString;
                });

                services.AddTransient<IConnectionMultiplexer>(
                    provider => ConnectionMultiplexer.Connect(dataCacheConnectionString));
                services.Decorate<ITextRetriever, RedisCachedTextRetriever>();
            }
            else
            {
                services.AddTransient<IInfrastructureDeploymentService, NoInfrastructureService>();
                services.Decorate<ITextRetriever, InMemoryCachedTextRetriever>();
            }

            return services;
        }
    }
}
