namespace TypingRealm.Messaging;

// TODO: Move to client project as server doesn't need this.
public interface IClientToServerMessageMetadataFactory
{
    ClientToServerMessageMetadata CreateFor(object message);
}
