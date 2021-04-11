namespace TypingRealm.Messaging
{
    public sealed class ClientToServerMessageMetadataFactory : IClientToServerMessageMetadataFactory
    {
        private readonly IMessageIdFactory _messageIdFactory;

        public ClientToServerMessageMetadataFactory(IMessageIdFactory messageIdFactory)
        {
            _messageIdFactory = messageIdFactory;
        }

        public ClientToServerMessageMetadata CreateFor(object message)
        {
            var metadata = ClientToServerMessageMetadata.CreateEmpty();
            metadata.MessageId = _messageIdFactory.CreateMessageId();

            // Load any metadata from attributes here.

            return metadata;
        }
    }
}
