using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Serialization.Connections;

// TODO: Unit test this class.
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
            throw new InvalidOperationException($"Received invalid message: {message.GetType().Name} is not a {typeof(MessageData).Name} type.");

        var data = messageData.Data;
        var messageType = _messageTypeCache.GetTypeById(messageData.TypeId);

        var deserialized = _serializer.Deserialize(data, messageType);

        if (messageData.Metadata == null)
            messageData.Metadata = ClientToServerMessageMetadata.CreateEmpty();

        return new MessageWithMetadata
        {
            Message = deserialized,
            Metadata = messageData.Metadata
        };
    }

    public ValueTask SendAsync(object message, CancellationToken cancellationToken)
    {
        MessageMetadata? metadata = null;
        if (message is MessageWithMetadata messageWithMetadata)
        {
            message = messageWithMetadata.Message;
            metadata = messageWithMetadata.Metadata;
        }

        var serialized = _serializer.Serialize(message);
        var messageTypeId = _messageTypeCache.GetTypeId(message.GetType());

        var result = new MessageData
        {
            Data = serialized,
            TypeId = messageTypeId,
            Metadata = metadata // Can be null.
        };

        return _connection.SendAsync(result, cancellationToken);
    }
}
