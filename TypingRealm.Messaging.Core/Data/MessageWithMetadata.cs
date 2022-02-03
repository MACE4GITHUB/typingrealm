namespace TypingRealm.Messaging;

/// <summary>
/// This is an abstraction over the message after deserialization and before
/// serialization. During in-app time (before transfering the object over the
/// wire) the instances of this object are passed between classes.
/// </summary>
public sealed class MessageWithMetadata
{
#pragma warning disable CS8618
    public MessageWithMetadata() { }
#pragma warning restore CS8618
    public MessageWithMetadata(object message)
    {
        Message = message;
    }
    public MessageWithMetadata(object message, MessageMetadata? metadata)
    {
        Message = message;
        Metadata = metadata;
    }

    public object Message { get; set; }
    public MessageMetadata? Metadata { get; set; }
}
