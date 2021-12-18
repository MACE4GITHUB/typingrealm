using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Texts.Generators;

namespace TypingRealm.Texts
{
    public delegate ITextGenerator TextGeneratorResolver(string language);

    public static class RegistrationExtensions
    {
        public static IServiceCollection AddTextsApi(this IServiceCollection services)
        {
            services.AddTransient<ITextGenerator, EnglishTextGenerator>();

            services.AddTransient<TextGeneratorResolver>(provider => language =>
            {
                var generator = provider.GetServices<ITextGenerator>()
                    .LastOrDefault(generator => generator.Language == language);

                if (generator == null)
                    throw new NotSupportedException($"Text generator for language {language} is not supported.");

                return generator;
            });

            return services;
        }
    }
}
