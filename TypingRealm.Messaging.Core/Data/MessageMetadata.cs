namespace TypingRealm.Messaging;

public class MessageMetadata
{
    /// <summary>
    /// Related message ID that was specified on the message before sending it
    /// to the other party. Can be null if not set. Can be used for
    /// acknowledgement flow.
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// When acknowledgement type is set, the receiving party should send an ack
    /// response after successfully receiving or handling the message, so that
    /// the sending party can be aware that it can continue sending next message.
    /// </summary>
    public AcknowledgementType AcknowledgementType { get; set; } = AcknowledgementType.Handled;

    /// <summary>
    /// If set, requests response with this type from the second party. Used for
    /// RPC calls, the second party should respond with the same MessageId.
    /// Shouldn't work if MessageId is not set as there's no other way to match
    /// response to the request.
    /// </summary>
    public string? ResponseMessageTypeId { get; set; }

    public static MessageMetadata CreateEmpty() => new MessageMetadata();
}
