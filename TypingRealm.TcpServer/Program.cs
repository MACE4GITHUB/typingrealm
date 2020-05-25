using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Protobuf;

namespace TypingRealm.TcpServer
{
    public static class Program
    {
        private const int Port = 30100;

        public static async Task Main()
        {
            var provider = new ServiceCollection()
                .AddSerializationCore()
                .AddProtobuf()
                .Services
                .RegisterMessaging()
                .AddLogging(builder => builder.AddConsole())
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace)
                .AddTcpServer(Port)
                .BuildServiceProvider();

            await using (var server = provider.GetRequiredService<TcpServer>())
            {
                server.Start();

                Console.WriteLine("=== TypingRealm server ===");
                Console.WriteLine("Press enter to stop listening.");
                Console.ReadLine();
            }

            Console.WriteLine("Stopped listening. Press enter to exit.");
            Console.ReadLine();
        }
    }

    public static class RegistrationExtensions
    {
        public static IServiceCollection AddTcpServer(this IServiceCollection services, int port)
            => services.AddTransient(provider => new TcpServer(
                port,
                provider.GetRequiredService<ILogger<TcpServer>>(),
                provider.GetRequiredService<IConnectionHandler>(),
                provider.GetRequiredService<IProtobufConnectionFactory>()));
    }
}
