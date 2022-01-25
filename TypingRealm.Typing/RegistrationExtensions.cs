using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Typing.Infrastructure;

namespace TypingRealm.Typing;

public static class RegistrationExtensions
{
    public static IServiceCollection AddTyping(this IServiceCollection services)
    {
        services.AddTransient<ITextTypingResultValidator, TextTypingResultValidator>();
        services.AddTransient<ITypingResultProcessor, TypingResultProcessor>();
        services.AddTransient<ITypingReportGenerator, TypingReportGenerator>();

        /*services.AddSingleton<ITextRepository, InMemoryTextRepository>();
        services.AddSingleton<ITypingSessionRepository, InMemoryTypingSessionRepository>();
        services.AddSingleton<IUserSessionRepository, InMemoryUserSessionRepository>();*/

        services.AddSingleton<IUserTypingStatisticsStore, InMemoryUserTypingStatisticsStore>();

        services.AddSingleton<ITextRepository, JsonFileTextRepository>();
        services.AddSingleton<ITypingSessionRepository, JsonFileTypingSessionRepository>();
        services.AddSingleton<IUserSessionRepository, JsonFileUserSessionRepository>();

        return services;
    }
}
