namespace TypingRealm.Messaging;

public static class MessageExtensions
{
    /// <summary>
    /// Gets metadata from object if it is <see cref="ClientToServerMessageWithMetadata"/>,
    /// otherwise returns empty <see cref="ClientToServerMessageMetadata"/>.
    /// </summary>
    public static MessageMetadata GetMetadataOrEmpty(this object message)
    {
        if (message is MessageWithMetadata messageWithMetadata
            && messageWithMetadata.Metadata != null)
            return messageWithMetadata.Metadata;

        return ClientToServerMessageMetadata.CreateEmpty();
    }
}
