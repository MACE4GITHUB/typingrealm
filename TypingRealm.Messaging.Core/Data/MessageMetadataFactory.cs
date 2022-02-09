namespace TypingRealm.Messaging;

public interface IMessageMetadataFactory
{
    MessageMetadata CreateFor(object message);
}

public sealed class MessageMetadataFactory : IMessageMetadataFactory
{
    private readonly IMessageIdFactory _messageIdFactory;

    public MessageMetadataFactory(IMessageIdFactory messageIdFactory)
    {
        _messageIdFactory = messageIdFactory;
    }

    public MessageMetadata CreateFor(object message)
    {
        var metadata = MessageMetadata.CreateEmpty();
        metadata.MessageId = _messageIdFactory.CreateMessageId();

        // Load any metadata from attributes here.

        return metadata;
    }
}
