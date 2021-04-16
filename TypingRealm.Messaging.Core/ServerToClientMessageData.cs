namespace TypingRealm.Messaging
{
    public sealed class ServerToClientMessageData : MessageData
    {
#pragma warning disable CS8618
        public ServerToClientMessageData() { }
#pragma warning restore CS8618

        public ServerToClientMessageMetadata? Metadata { get; set; }
    }
}
