namespace TypingRealm.Messaging;

// TODO: Move to client project as server doesn't need this.
public interface IMessageMetadataFactory
{
    MessageMetadata CreateFor(object message);
}
