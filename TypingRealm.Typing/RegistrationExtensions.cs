using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Typing.Infrastructure;

namespace TypingRealm.Typing
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddTyping(this IServiceCollection services)
        {
            services.AddTransient<ITextTypingResultValidator, TextTypingResultValidator>();
            services.AddTransient<ITypingResultProcessor, TypingResultProcessor>();

            services.AddTransient<ITextRepository, InMemoryTextRepository>();
            services.AddTransient<ITypingSessionRepository, InMemoryTypingSessionRepository>();
            services.AddTransient<IUserSessionRepository, InMemoryUserSessionRepository>();

            return services;
        }
    }
}
