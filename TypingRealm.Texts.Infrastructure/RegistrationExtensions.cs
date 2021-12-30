using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Communication;
using TypingRealm.Texts.Retrievers.Cache;

namespace TypingRealm.Texts.Infrastructure
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddTextsApi(this IServiceCollection services)
        {
            services.AddTextsDomain();

            if (DebugHelpers.UseInfrastructure)
            {
                services.AddTransient<ITextCache>(provider => new TextCache(
                    provider.GetRequiredService<IServiceCacheProvider>(), "en"));

                services.AddTransient<ITextCache>(provider => new TextCache(
                    provider.GetRequiredService<IServiceCacheProvider>(), "ru"));
            }

            return services;
        }
    }
}
