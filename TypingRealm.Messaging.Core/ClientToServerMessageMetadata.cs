namespace TypingRealm.Messaging
{
    public sealed class ClientToServerMessageMetadata
    {
        public string? MessageId { get; set; }
        public bool RequireAcknowledgement { get; set; }

        public static ClientToServerMessageMetadata CreateEmpty() => new ClientToServerMessageMetadata();

        public void EnableAcknowledgement(string messageId)
        {
            MessageId = messageId;
            RequireAcknowledgement = true;
        }
    }
}
