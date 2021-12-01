using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TypingRealm.Messaging.Serialization;

namespace TypingRealm.Hosting
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

        public static WebApplicationBuilder CreateWebApiApplicationBuilder(Assembly controllersAssembly)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.UseWebApiHost(controllersAssembly);

            return builder;
        }

        public static IHostBuilder CreateWebApiHostBuilder(
            Assembly controllersAssembly,
            Action<IServiceCollection> configureServices,
            Action<IApplicationBuilder>? configureApp = null)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.UseWebApiHost(controllersAssembly);
                        configureServices(services);
                    });

                    webBuilder.Configure(app =>
                    {
                        configureApp?.Invoke(app);
                    });
                });
        }
    }
}
