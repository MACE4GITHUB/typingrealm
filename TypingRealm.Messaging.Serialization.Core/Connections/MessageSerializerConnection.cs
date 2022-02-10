using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Serialization.Connections;

public sealed class MessageSerializerConnection : IConnection
{
    private readonly IConnection _connection;
    private readonly IMessageSerializer _serializer;
    private readonly IMessageTypeCache _messageTypeCache;
    private readonly IMessageMetadataFactory _metadataFactory;

    public MessageSerializerConnection(
        IConnection connection,
        IMessageSerializer serializer,
        IMessageTypeCache messageTypeCache,
        IMessageMetadataFactory metadataFactory)
    {
        _connection = connection;
        _serializer = serializer;
        _messageTypeCache = messageTypeCache;
        _metadataFactory = metadataFactory;
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

        // TODO: Consider using MetadataFactory here (but set messageId to null, because it wasn't passed).
        // Probably best to implement CreateEmpty() method on MetadataFactory.
        if (messageData.Metadata == null)
            messageData.Metadata = MessageMetadata.CreateEmpty();

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

        // If metadata was sent from user side - use user's metadata, otherwise create for this message.
        if (metadata == null)
            metadata = _metadataFactory.CreateFor(message);

        var result = new MessageData
        {
            Data = serialized,
            TypeId = messageTypeId,
            Metadata = metadata
        };

        return _connection.SendAsync(result, cancellationToken);
    }
}
