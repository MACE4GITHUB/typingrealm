using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Messaging.Serialization.Protobuf;

namespace TypingRealm.RopeWar.TcpServer
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddTcpServer(this IServiceCollection services, int port)
            => services.AddTransient(provider => new TcpServer(
                port,
                provider.GetRequiredService<ILogger<TcpServer>>(),
                provider.GetRequiredService<IConnectionHandler>(),
                provider.GetRequiredService<IProtobufConnectionFactory>(),
                provider.GetRequiredService<IJsonConnectionFactory>()));
    }
}
