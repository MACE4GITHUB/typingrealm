namespace TypingRealm.Messaging.Messages;

/// <summary>
/// Messages derived from this type will be automatically sent to all the
/// clients (excluding the sender) in the same messaging group as the sender.
/// This is mostly used for event effects (for the client).
/// </summary>
[Message]
public abstract class BroadcastMessage
{
#pragma warning disable CS8618
    protected BroadcastMessage() { }
#pragma warning restore CS8618
    protected BroadcastMessage(string senderId) => SenderId = senderId;

    public string SenderId { get; set; }
}
