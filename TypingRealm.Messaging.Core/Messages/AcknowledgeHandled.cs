namespace TypingRealm.Messaging.Messages;

[Message]
public sealed class AcknowledgeHandled
{
#pragma warning disable CS8618
    public AcknowledgeHandled() { }
#pragma warning restore CS8618
    public AcknowledgeHandled(string messageId)
    {
        MessageId = messageId;
    }

    public string MessageId { get; set; }
}
