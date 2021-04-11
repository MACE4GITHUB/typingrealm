using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Serialization.Connections
{
    public sealed class ServerToClientSendingMessageSerializerConnection : IConnection
    {
        private readonly IConnection _connection;
        private readonly IMessageSerializer _serializer;
        private readonly IMessageTypeCache _messageTypeCache;

        public ServerToClientSendingMessageSerializerConnection(
            IConnection connection,
            IMessageSerializer serializer,
            IMessageTypeCache messageTypeCache)
        {
            _connection = connection;
            _serializer = serializer;
            _messageTypeCache = messageTypeCache;
        }

        public async ValueTask<object> ReceiveAsync(CancellationToken cancellationToken)
        {
            var message = await _connection.ReceiveAsync(cancellationToken)
                .ConfigureAwait(false);

            if (message is not MessageData messageData)
                throw new InvalidOperationException("Received invalid message: not a MessageData type.");

            var data = messageData.Data;
            var messageType = _messageTypeCache.GetTypeById(messageData.TypeId);

            var deserialized = _serializer.Deserialize(data, messageType);

            if (message is ClientToServerMessageData clientMessageData)
            {
                return new ClientToServerMessageWithMetadata
                {
                    Message = deserialized,
                    Metadata = clientMessageData.Metadata
                };
            }

            return new ClientToServerMessageWithMetadata
            {
                Message = deserialized,
                Metadata = ClientToServerMessageMetadata.CreateEmpty()
            };
        }

        public ValueTask SendAsync(object message, CancellationToken cancellationToken)
        {
            var serialized = _serializer.Serialize(message);
            var messageTypeId = _messageTypeCache.GetTypeId(message.GetType());

            var result = new ServerToClientMessageData
            {
                Data = serialized,
                TypeId = messageTypeId
            };

            return _connection.SendAsync(result, cancellationToken);
        }
    }
}
