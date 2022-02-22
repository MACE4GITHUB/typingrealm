using System.Reflection;

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
        // TODO: Cache types & attributes.
        var messageAttribute = message.GetType().GetCustomAttribute<MessageAttribute>();
        if (messageAttribute != null)
        {
            metadata.SendUpdate = messageAttribute.SendUpdate;
        }

        return metadata;
    }
}
