using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Serialization.Connections;

/// <summary>
/// Use this connection at client side (it should send messages to the server).
/// </summary>
// TODO: Unit test this class.
public sealed class ClientToServerSendingMessageSerializerConnection : IConnection
{
    private readonly IConnection _connection;
    private readonly IMessageSerializer _serializer;
    private readonly IMessageTypeCache _messageTypeCache;
    private readonly IClientToServerMessageMetadataFactory _metadataFactory;

    public ClientToServerSendingMessageSerializerConnection(
        IConnection connection,
        IMessageSerializer serializer,
        IMessageTypeCache messageTypeCache,
        IClientToServerMessageMetadataFactory metadataFactory)
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

        if (message is not ServerToClientMessageData messageData)
            throw new InvalidOperationException($"Received invalid message: {message.GetType().Name} is not a {typeof(ServerToClientMessageData).Name} type.");

        var data = messageData.Data;
        var messageType = _messageTypeCache.GetTypeById(messageData.TypeId);

        var deserialized = _serializer.Deserialize(data, messageType);

        if (messageData.Metadata == null)
            messageData.Metadata = ServerToClientMessageMetadata.CreateEmpty();

        return new MessageWithMetadata
        {
            Message = deserialized,
            Metadata = messageData.Metadata
        };
    }

    public ValueTask SendAsync(object message, CancellationToken cancellationToken)
    {
        ClientToServerMessageMetadata? metadata = null;
        if (message is MessageWithMetadata messageWithMetadata)
        {
            message = messageWithMetadata.Message;
            metadata = messageWithMetadata.Metadata as ClientToServerMessageMetadata;
        }

        var serialized = _serializer.Serialize(message);
        var messageTypeId = _messageTypeCache.GetTypeId(message.GetType());

        // TODO: Remove this code and generate metadata above - where we need to save messageId for idempotency (retries).
        // If metadata was sent from user side - use user's metadata, otherwise create for this message.
        /*if (metadata == null)
            metadata = _metadataFactory.CreateFor(message);*/

        var result = new ClientToServerMessageData
        {
            Data = serialized,
            TypeId = messageTypeId,
            Metadata = metadata
        };

        return _connection.SendAsync(result, cancellationToken);
    }
}
