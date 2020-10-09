using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TypingRealm.Messaging.Serialization;

namespace TypingRealm.Hosting
{
    public static class HostFactory
    {
        public static IHostBuilder CreateSignalRHostBuilder(Action<MessageTypeCacheBuilder> configure)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        var messageTypeCacheBuilder = services.UseSignalRHost();
                        configure(messageTypeCacheBuilder);
                    });

                    webBuilder.Configure(app => { });
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

        public static IHostBuilder CreateWebApiHostBuilder(Assembly controllersAssembly, Action<IServiceCollection> configure)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.UseWebApiHost(controllersAssembly);
                        configure(services);
                    });

                    webBuilder.Configure(app => { });
                });
        }
    }

}
