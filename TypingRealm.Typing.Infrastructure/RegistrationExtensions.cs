using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TypingRealm.Communication.Redis;
using TypingRealm.DataAccess.Postgres;
using TypingRealm.Texts.Api.Client;
using TypingRealm.Typing.Infrastructure.DataAccess;
using TypingRealm.Typing.Infrastructure.DataAccess.Repositories;

namespace TypingRealm.Typing.Infrastructure;

public static class RegistrationExtensions
{
    public static IServiceCollection AddTypingApi(
        this IServiceCollection services,
        IConfiguration configuration,
        string cacheConnectionString,
        string dataCacheConnectionString)
    {
        _ = cacheConnectionString;

        // Typing Domain.
        services.AddTyping();

        // API client to Texts service.
        services.AddTextsApiClient();

        // Typing.
        services.AddSingleton<ITextGenerator, TextGenerator>();

        services.AddTransient<ITextRetriever, QuotableTextRetriever>();

        // Repositories.
        if (DebugHelpers.UseInfrastructure)
        {
            services.AddTransient<ITextRepository, TextRepository>();
            services.AddTransient<ITypingSessionRepository, TypingSessionRepository>();
            services.AddTransient<IUserSessionRepository, UserSessionRepository>();

            services.AddPostgresWithEFMigrations<DataContext>(configuration);

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
            services.Decorate<ITextRetriever, InMemoryCachedTextRetriever>(ServiceLifetime.Singleton);
        }

        return services;
    }
}
