namespace TypingRealm.Messaging
{
    public static class MessageExtensions
    {
        /// <summary>
        /// Gets metadata from object if it is <see cref="ClientToServerMessageWithMetadata"/>,
        /// otherwise returns empty <see cref="ClientToServerMessageMetadata"/>.
        /// </summary>
        public static ClientToServerMessageMetadata GetMetadataOrEmpty(this object message)
        {
            if (message is ClientToServerMessageWithMetadata messageWithMetadata)
                return messageWithMetadata.Metadata;

            return ClientToServerMessageMetadata.CreateEmpty();
        }
    }
}
