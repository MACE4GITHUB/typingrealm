using Microsoft.Extensions.DependencyInjection;
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

            services.AddTextRetrieverCache<EnglishTextRetriever>("en");
            services.AddTextRetrieverCache<RussianTextRetriever>("ru");

            return services;
        }

        private static IServiceCollection AddTextRetrieverCache<TTextRetriever>(
            this IServiceCollection services, string language)
            where TTextRetriever : ITextRetriever
        {
            // The retriever cache can be transient only if we inject a singleton local lock.
            services.AddSingleton<ITextRetriever>(provider => new CachedTextRetriever(
                provider.GetRequiredService<TTextRetriever>(),
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
