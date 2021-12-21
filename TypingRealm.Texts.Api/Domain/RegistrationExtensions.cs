using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Communication;
using TypingRealm.Texts.Generators;

namespace TypingRealm.Texts;

public delegate ITextRetriever TextRetrieverResolver(string language);

public static class RegistrationExtensions
{
    public static IServiceCollection AddTextsApi(this IServiceCollection services)
    {
        // The retriever cache can be transient only if we inject a singleton local lock.

        services.AddTransient<ITextGenerator, TextGenerator>();

        services.AddTransient<EnglishTextRetriever>();
        services.AddSingleton<ITextRetriever>(provider => new CachedTextRetriever(
            provider.GetRequiredService<EnglishTextRetriever>(),
            provider.GetRequiredService<IServiceCacheProvider>()));

        services.AddTransient<RussianTextRetriever>();
        services.AddSingleton<ITextRetriever>(provider => new CachedTextRetriever(
            provider.GetRequiredService<RussianTextRetriever>(),
            provider.GetRequiredService<IServiceCacheProvider>()));

        services.AddTransient<TextRetrieverResolver>(provider => language =>
        {
            var retriever = provider.GetServices<ITextRetriever>()
                .LastOrDefault(generator => generator.Language == language);

            if (retriever == null)
                throw new NotSupportedException($"Text generator for language {language} is not supported.");

            return new CachedTextRetriever(
                retriever,
                provider.GetRequiredService<IServiceCacheProvider>());
        });

        return services;
    }
}
