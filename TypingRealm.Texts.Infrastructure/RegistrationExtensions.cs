using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TypingRealm.Communication;
using TypingRealm.Texts.Retrievers;
using TypingRealm.Texts.Retrievers.Cache;

namespace TypingRealm.Texts.Infrastructure
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddTextsApi(this IServiceCollection services)
        {
            services.AddTextsDomain();

            foreach (var config in SupportedLanguages.SupportedTextRetrievers)
            {
                if (config.Value.Key == typeof(LibraryTextRetriever))
                    continue; // Do not add caching if retriever is library retriever.

                services.AddTextRetrieverCache(config.Value.Key, config.Key);
            }

            return services;
        }

        private static IServiceCollection AddTextRetrieverCache(
            this IServiceCollection services, Type textRetrieverType, string language)
        {
            // The retriever cache can be transient only if we inject a singleton local lock.
            services.AddSingleton<ITextRetriever>(provider => new CachedTextRetriever(
                provider.GetRequiredService<ILogger<CachedTextRetriever>>(),
                (ITextRetriever)provider.GetRequiredService(textRetrieverType),
                provider.GetRequiredService<TextCacheResolver>()(language)));

            if (DebugHelpers.UseInfrastructure)
            {
                return services.AddTransient<ITextCache>(provider => new TextCache(
                    provider.GetRequiredService<IServiceCacheProvider>(), language));
            }

            return services.AddSingleton<ITextCache>(provider => new InMemoryTextCache(language));
        }
    }
}
