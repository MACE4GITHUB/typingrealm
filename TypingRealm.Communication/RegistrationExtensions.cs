using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TypingRealm.Authentication;

namespace TypingRealm.Communication;

public static class RegistrationExtensions
{
    public static IServiceCollection AddCommunication(this IServiceCollection services)
    {
        services.TryAddTransient<IProfileTokenService, AnonymousProfileTokenService>();
        services.TryAddTransient<IServiceTokenService, AnonymousServiceTokenService>();

        services.AddTransient<IServiceClient, InMemoryServiceClient>();

        // Consider splitting this part to a separate TypingRealm.Caching project.
        services.AddCaching();

        return services;
    }

    public static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddMemoryCache();

        // We are using ITyrCache for SignalR Authentication tokens generation,
        // so we need it registered even for the services that don't have Redis.
        /*(if (!DebugHelpers.UseInfrastructure)
        {*/
            services.AddSingleton<InMemoryTyrCache>();
            services.AddSingleton<ITyrCache>(provider => provider.GetRequiredService<InMemoryTyrCache>());
            services.AddTransient<IDistributedLockProvider>(provider => provider.GetRequiredService<InMemoryTyrCache>());
        //}

        return services;
    }
}
