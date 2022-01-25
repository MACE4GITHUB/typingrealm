using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging.Messages;

namespace TypingRealm.Messaging.Serialization;

public static class RegistrationExtensions
{
    /// <summary>
    /// Registers IMessageTypeCache as singleton and returns new instance of
    /// <see cref="MessageTypeCacheBuilder"/> class that has core Messaging
    /// messages registered in it. This builder will be used it the time
    /// when <see cref="MessageTypeCache"/> is first requested.
    /// </summary>
    public static MessageTypeCacheBuilder AddSerializationCore(this IServiceCollection services)
    {
        // TODO: Register these only on client side. Server doesn't need generators of metadata.
        // !!! but then we also need to split client and server versions of protobuf/signalr connection factories, so it's easier to just always register these types.
        services.AddTransient<IClientToServerMessageMetadataFactory, ClientToServerMessageMetadataFactory>();

        // This is used only from the client so no need to register it as Scoped.
        // And it won't work as scoped because it is injected into singletone TCP/SignalR Server classes.
        services.AddSingleton<IMessageIdFactory, MessageIdFactory>();

        return new MessageTypeCacheBuilder(services)
            .AddMessageTypesFromAssembly(typeof(Disconnect).Assembly);
    }
}
