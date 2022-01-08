using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TypingRealm.Communication.Redis;
using TypingRealm.Data.Infrastructure.DataAccess;
using TypingRealm.Data.Infrastructure.DataAccess.Repositories;
using TypingRealm.Texts.Api.Client;
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
            _ = cacheConnectionString;

            // Typing Domain.
            services.AddTyping();

            // API client to Texts service.
            services.AddTextsApiClient();

            services.AddSingleton<ILocationRepository, InMemoryLocationRepository>();

            // Typing.
            services.AddSingleton<ITextGenerator, TextGenerator>();

            services.AddTransient<ITextRetriever, QuotableTextRetriever>();

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
                    options.Configuration = dataCacheConnectionString;
                });

                services.AddTransient<IConnectionMultiplexer>(
                    provider => ConnectionMultiplexer.Connect(dataCacheConnectionString));
                services.Decorate<ITextRetriever, RedisCachedTextRetriever>(ServiceLifetime.Singleton);

                services.AddRedisServiceCaching(dataCacheConnectionString);
            }
            else
            {
                services.AddTransient<IInfrastructureDeploymentService, NoInfrastructureService>();
                services.Decorate<ITextRetriever, InMemoryCachedTextRetriever>(ServiceLifetime.Singleton);
            }

            return services;
        }
    }
}
