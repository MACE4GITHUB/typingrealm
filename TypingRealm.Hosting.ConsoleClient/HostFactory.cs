using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TypingRealm.Messaging.Serialization;

namespace TypingRealm.Hosting.ConsoleClient;

// TODO: Make sure everything is aligned with TypingRealm.Hosting.HostFactory.
public static class HostFactory
{
    public static IHostBuilder CreateConsoleAppHostBuilder(Action<MessageTypeCacheBuilder>? configure = null)
    {
        return Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();

                    // TODO: Add some non-console logging provider to accumulate logs.
                    // (like user's TEMP folder)
                })
            .ConfigureServices(services =>
            {
                var messageTypeCacheBuilder = services.UseConsoleAppHost();
                configure?.Invoke(messageTypeCacheBuilder);
            });
    }
}
