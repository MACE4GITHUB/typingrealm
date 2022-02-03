namespace TypingRealm.Messaging;

public sealed class ServerToClientMessageMetadata : MessageMetadata
{
    public static ServerToClientMessageMetadata CreateEmpty() => new ServerToClientMessageMetadata();
}
