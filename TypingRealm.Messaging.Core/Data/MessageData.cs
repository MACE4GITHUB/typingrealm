namespace TypingRealm.Messaging;

/// <summary>
/// Instance of this class is transfered over the network.
/// The actual message serialized payload is stored in the Data field.
/// </summary>
public sealed class MessageData
{
#pragma warning disable CS8618
    public MessageData() { }
#pragma warning restore CS8618

    public string Data { get; set; }
    public string TypeId { get; set; }
    public MessageMetadata? Metadata { get; set; }
}
