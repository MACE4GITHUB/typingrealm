using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TypingRealm.Configuration;
using TypingRealm.Logging;
using TypingRealm.Messaging.Serialization;

namespace TypingRealm.Hosting.Service;

public static class HostFactory
{
    public static WebApplicationBuilder CreateSignalRApplicationBuilder(Assembly controllersAssembly)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddTyrConfiguration();
        builder.Logging.AddTyrLogging(builder.Configuration);
        builder.Services.UseSignalRHost(builder.Configuration, controllersAssembly);

        return builder;
    }

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
                    (context, loggingBuilder) => loggingBuilder.AddTyrLogging(context.Configuration));

                webBuilder.ConfigureAppConfiguration(configBuilder => configBuilder.AddTyrConfiguration());

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
            .ConfigureLogging((context, loggingBuilder) => loggingBuilder.AddTyrLogging(context.Configuration))
            .ConfigureAppConfiguration(configBuilder => configBuilder.AddTyrConfiguration())
            .ConfigureServices(services =>
            {
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

                var messageTypeCacheBuilder = services.UseTcpHost(configuration, port);
                configure(messageTypeCacheBuilder);
            });
    }
}
