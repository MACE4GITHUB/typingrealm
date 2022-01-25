namespace TypingRealm.Messaging;

public sealed class ClientToServerMessageData : MessageData
{
#pragma warning disable CS8618
    public ClientToServerMessageData() { }
#pragma warning restore CS8618

    public ClientToServerMessageMetadata? Metadata { get; set; }
}
