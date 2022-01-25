using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging.Client;
using TypingRealm.Messaging.Serialization.Protobuf;

namespace TypingRealm.Tcp.Client;

public static class RegistrationExtensions
{
    public static IServiceCollection UseTcpProtobufClientConnectionFactory(
        this IServiceCollection services, string host, int port)
    {
        services.AddTransient<IClientConnectionFactory>(provider =>
        {
            var factory = provider.GetRequiredService<IProtobufConnectionFactory>();

            return new TcpProtobufClientConnectionFactory(factory, host, port);
        });

        return services;
    }
}
