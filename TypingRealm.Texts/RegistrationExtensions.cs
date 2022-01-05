using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Library.Api.Client;
using TypingRealm.Texts.Retrievers;
using TypingRealm.Texts.Retrievers.Cache;

namespace TypingRealm.Texts;

public delegate ITextRetriever TextRetrieverResolver(string language);
public delegate ITextCache TextCacheResolver(string language);

public static class RegistrationExtensions
{
    public static IServiceCollection AddTextsDomain(this IServiceCollection services)
    {
        services.AddLibraryApiClients();

        services.AddTransient<ITextGenerator, TextGenerator>();

        if (DebugHelpers.UseInfrastructure)
        {
            foreach (var config in SupportedLanguages.SupportedTextRetrievers)
            {
                services.AddTransient(config.Value);
                services.AddTransient(typeof(ITextRetriever), config.Value);
            }
        }
        else
        {
            // Adds only English and Russian hardcoded text retrievers.
            services.AddHardcodedTextRetrieverStubs();
        }

        services.AddTransient<TextRetrieverResolver>(provider => language =>
        {
            var retriever = provider.GetServices<ITextRetriever>()
                .LastOrDefault(retriever => retriever.Language == language);

            return retriever ?? throw new NotSupportedException($"Text generator for language {language} is not supported.");
        });

        services.AddTransient<TextCacheResolver>(provider => language =>
        {
            var cache = provider.GetServices<ITextCache>()
                .LastOrDefault(cache => cache.Language == language);

            return cache ?? throw new NotSupportedException($"Text cache for language {language} is not registered.");
        });

        return services;
    }

    private static IServiceCollection AddHardcodedTextRetrieverStubs(this IServiceCollection services)
    {
        services.AddSingleton<ITextRetriever>(new OfflineTextRetriever("en", new[]
        {
            "Love is rarer than genius itself. And friendship is rarer than love.",
            "Don't leave a stone unturned. It's always something, to know you have done the most you could.",
            "Never, never, never give up."
        }));

        services.AddSingleton<ITextRetriever>(new OfflineTextRetriever("ru", new[]
        {
            "Высокий уровень вовлечения представителей целевой аудитории является четким доказательством простого факта: дальнейшее развитие различных форм деятельности является качественно новой ступенью своевременного выполнения сверхзадачи!",
            "Есть над чем задуматься: сделанные на базе интернет-аналитики выводы, превозмогая сложившуюся непростую экономическую ситуацию, функционально разнесены на независимые элементы.",
            "Кстати, базовые сценарии поведения пользователей призывают нас к новым свершениям, которые, в свою очередь, должны быть объединены в целые кластеры себе подобных. В частности, новая модель организационной деятельности, а также свежий взгляд на привычные вещи - безусловно открывает новые горизонты для укрепления моральных ценностей."
        }));

        return services;
    }
}
