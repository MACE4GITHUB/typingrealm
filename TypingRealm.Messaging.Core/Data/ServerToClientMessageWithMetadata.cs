namespace TypingRealm.Messaging;

public sealed class ServerToClientMessageWithMetadata
{
#pragma warning disable CS8618
    public ServerToClientMessageWithMetadata() { }
#pragma warning restore CS8618
    public ServerToClientMessageWithMetadata(
        object message,
        ServerToClientMessageMetadata metadata)
    {
        Message = message;
        Metadata = metadata;
    }

    public object Message { get; set; }
    public ServerToClientMessageMetadata Metadata { get; set; }
}
