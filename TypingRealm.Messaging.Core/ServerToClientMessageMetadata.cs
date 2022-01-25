namespace TypingRealm.Messaging;

public sealed class ServerToClientMessageMetadata
{
    /// <summary>
    /// Related message ID that was specified on the message sent from the
    /// client to the server.
    /// </summary>
    public string? RequestMessageId { get; set; }

    public static ServerToClientMessageMetadata CreateEmpty() => new ServerToClientMessageMetadata();
}
