using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TypingRealm.Authentication;
using TypingRealm.Communication;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Messaging.Serialization.Protobuf;

namespace TypingRealm.RopeWar.TcpServer
{
    public static class Program
    {
        private const int Port = 30102;

        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddCommunication();
                    services.AddTyrServiceWithoutAspNetAuthentication()
                        .UseLocalProvider();

                    services.AddSerializationCore()
                        .AddMessageTypesFromAssembly(typeof(JoinContest).Assembly) // TODO: Move to RegisterRopeWar?
                        .AddJson()
                        .Services
                        .AddProtobuf()
                        .RegisterMessaging()
                        .RegisterRopeWar()
                        .AddTcpServer(Port);

                    services.AddHostedService<RopeWarTcpServer>();
                });
    }
}
