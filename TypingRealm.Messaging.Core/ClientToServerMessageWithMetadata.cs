namespace TypingRealm.Messaging
{
    public sealed class ClientToServerMessageWithMetadata
    {
#pragma warning disable CS8618
        public ClientToServerMessageWithMetadata() { }
#pragma warning restore CS8618

        public object Message { get; set; }
        public ClientToServerMessageMetadata Metadata { get; set; }
    }
}
