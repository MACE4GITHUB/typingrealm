namespace TypingRealm.Messaging;

// HACK: We need this attribute because ProtobufStreamSerializer still relies on MessageTypeCache.
[Message]
public abstract class MessageData
{
#pragma warning disable CS8618
    protected MessageData() { }
#pragma warning restore CS8618

    public string Data { get; set; }
    public string TypeId { get; set; }
}

public abstract class MessageData<TMetadata> : MessageData
{
    public TMetadata? Metadata { get; set; }
}

public sealed class MessageWithMetadata
{
#pragma warning disable CS8618
    public MessageWithMetadata() { }
#pragma warning restore CS8618
    public MessageWithMetadata(object message, MessageMetadata metadata)
    {
        Message = message;
        Metadata = metadata;
    }

    public object Message { get; set; }
    public MessageMetadata Metadata { get; set; }
}

public sealed class ClientToServerMessageData : MessageData<ClientToServerMessageMetadata>
{
}

public sealed class ServerToClientMessageData : MessageData<MessageMetadata>
{
}
