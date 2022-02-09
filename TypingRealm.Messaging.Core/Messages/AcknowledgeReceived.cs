namespace TypingRealm.Messaging.Messages;

[Message]
public sealed class AcknowledgeReceived
{
#pragma warning disable CS8618
    public AcknowledgeReceived() { }
#pragma warning restore CS8618
    public AcknowledgeReceived(string messageId)
    {
        MessageId = messageId;
    }

    public string MessageId { get; set; }
}
