namespace TypingRealm.Messaging;

public interface IClientToServerMessageMetadataFactory
{
    ClientToServerMessageMetadata CreateFor(object message);
}
