using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Texts.Generators;

namespace TypingRealm.Texts
{
    public delegate ITextRetriever TextRetrieverResolver(string language);

    public static class RegistrationExtensions
    {
        public static IServiceCollection AddTextsApi(this IServiceCollection services)
        {
            services.AddTransient<ITextRetriever, EnglishTextRetriever>()
                // This can be transient only if we inject a singleton local lock.
                .Decorate<ITextRetriever, CachedTextRetriever>(ServiceLifetime.Singleton);

            services.AddTransient<TextRetrieverResolver>(provider => language =>
            {
                var retriever = provider.GetServices<ITextRetriever>()
                    .LastOrDefault(generator => generator.Language == language);

                if (retriever == null)
                    throw new NotSupportedException($"Text generator for language {language} is not supported.");

                return retriever;
            });

            return services;
        }
    }
}
