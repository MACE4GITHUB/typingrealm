using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Typing
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddTyping(this IServiceCollection services)
        {
            services.AddTransient<ITextValidator, TextValidator>();
            services.AddTransient<ITypedTextProcessor, TypedTextProcessor>();
            services.AddTransient<ITextTypingStatisticsCalculator, TextTypingStatisticsCalculator>();

            return services;
        }
    }
}
