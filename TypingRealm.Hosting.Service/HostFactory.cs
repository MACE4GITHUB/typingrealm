using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using TypingRealm.Messaging.Serialization;

namespace TypingRealm.Hosting.Service
{
    public static class HostFactory
    {
        public static IHostBuilder CreateSignalRHostBuilder(
            Action<MessageTypeCacheBuilder> configureServices,
            Action<IApplicationBuilder>? configureApp = null)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        var messageTypeCacheBuilder = services.UseSignalRHost();
                        configureServices(messageTypeCacheBuilder);
                    });

                    webBuilder.Configure(app =>
                    {
                        configureApp?.Invoke(app);
                    });
                });
        }

        public static IHostBuilder CreateTcpHostBuilder(int port, Action<MessageTypeCacheBuilder> configure)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    var messageTypeCacheBuilder = services.UseTcpHost(port);
                    configure(messageTypeCacheBuilder);
                });
        }
    }
}
