using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Serialization.Protobuf;

namespace TypingRealm.Tcp
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddTcpServer(this IServiceCollection services, int port)
            => services.AddTransient(provider => new Tcp.TcpServer(
                port,
                provider.GetRequiredService<ILogger<Tcp.TcpServer>>(),
                provider.GetRequiredService<IScopedConnectionHandler>(),
                provider.GetRequiredService<IProtobufConnectionFactory>()));
    }
}
