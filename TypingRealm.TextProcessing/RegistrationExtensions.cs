using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.TextProcessing
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddTextProcessing(this IServiceCollection services)
        {
            services.AddSingleton<ILanguageProvider, LanguageProvider>();
            services.AddTransient<ITextProcessor, TextProcessor>();

            return services;
        }
    }
}
