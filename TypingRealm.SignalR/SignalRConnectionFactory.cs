using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connections;
using TypingRealm.Messaging.Serialization;
using TypingRealm.SignalR.Connections;

namespace TypingRealm.SignalR;

public sealed class SignalRConnectionFactory : ISignalRConnectionFactory
{
    private readonly IMessageSerializer _messageSerializer;
    private readonly IMessageTypeCache _messageTypeCache;
    private readonly IMessageMetadataFactory _messageMetadataFactory;

    public SignalRConnectionFactory(
        IMessageSerializer messageSerializer,
        IMessageTypeCache messageTypeCache,
        IMessageMetadataFactory clientToServerMessageMetadataFactory)
    {
        _messageSerializer = messageSerializer;
        _messageTypeCache = messageTypeCache;
        _messageMetadataFactory = clientToServerMessageMetadataFactory;
    }

    public IConnection CreateProtobufConnectionForClient(HubConnection hub)
    {
        return ClientToServerSignalRMessageSender.Create(hub)
            .AddCoreMessageSerialization(_messageSerializer, _messageTypeCache, _messageMetadataFactory);
    }

    public IConnection CreateProtobufConnectionForServer(IClientProxy clientProxy, Notificator notificator)
    {
        return new ServerToClientSignalRMessageSender(clientProxy)
            .WithNotificator(notificator)
            .AddCoreMessageSerialization(_messageSerializer, _messageTypeCache, _messageMetadataFactory);
    }
}
