using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TypingRealm.Logging;
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
                        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

                        var messageTypeCacheBuilder = services.UseSignalRHost(configuration);
                        configureServices(messageTypeCacheBuilder);
                    });

                    webBuilder.ConfigureLogging(
                        loggingBuilder => loggingBuilder.AddTyrLogging());

                    webBuilder.Configure(app =>
                    {
                        configureApp?.Invoke(app);
                    });
                });
        }

        public static IHostBuilder CreateTcpHostBuilder(int port, Action<MessageTypeCacheBuilder> configure)
        {
            // TODO: Figure out how to set up logs here, we can't call ConfigureWebHostDefaults as it's not a Web service.
            // At least call ConfigureTyrLogging method.

            return Host.CreateDefaultBuilder()
                .ConfigureLogging(loggingBuilder => loggingBuilder.AddTyrLogging())
                .ConfigureServices(services =>
                {
                    var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

                    var messageTypeCacheBuilder = services.UseTcpHost(configuration, port);
                    configure(messageTypeCacheBuilder);
                });
        }
    }
}
