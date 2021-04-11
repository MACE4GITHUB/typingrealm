namespace TypingRealm.Messaging
{
    // HACK: We need this attribute because ProtobufStreamSerializer still relies on MessageTypeCache.
    [Message]
    public abstract class MessageData
    {
#pragma warning disable CS8618
        protected MessageData() { }
#pragma warning restore CS8618

        public string Data { get; set; }
        public string TypeId { get; set; }
    }
}
