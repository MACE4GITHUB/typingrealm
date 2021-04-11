using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connections;
using TypingRealm.Messaging.Serialization;
using TypingRealm.SignalR.Connections;

namespace TypingRealm.SignalR
{
    public sealed class SignalRConnectionFactory : ISignalRConnectionFactory
    {
        private readonly IMessageSerializer _messageSerializer;
        private readonly IMessageTypeCache _messageTypeCache;
        private readonly IClientToServerMessageMetadataFactory _clientToServerMessageMetadataFactory;

        public SignalRConnectionFactory(
            IMessageSerializer messageSerializer,
            IMessageTypeCache messageTypeCache,
            IClientToServerMessageMetadataFactory clientToServerMessageMetadataFactory)
        {
            _messageSerializer = messageSerializer;
            _messageTypeCache = messageTypeCache;
            _clientToServerMessageMetadataFactory = clientToServerMessageMetadataFactory;
        }

        public IConnection CreateProtobufConnectionForClient(HubConnection hub)
        {
            return ClientToServerSignalRMessageSender.Create(hub)
                .ForClient(_messageSerializer, _messageTypeCache, _clientToServerMessageMetadataFactory);
        }

        public IConnection CreateProtobufConnectionForServer(IClientProxy clientProxy, Notificator notificator)
        {
            return new ServerToClientSignalRMessageSender(clientProxy)
                .WithNotificator(notificator)
                .ForServer(_messageSerializer, _messageTypeCache);
        }
    }
}
