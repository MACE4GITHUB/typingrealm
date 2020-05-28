namespace TypingRealm.Messaging.Serialization.Json
{
    [Message]
    public sealed class JsonSerializedMessage
    {
#pragma warning disable CS8618
        public JsonSerializedMessage() { }
#pragma warning restore CS8618
        public JsonSerializedMessage(string typeId, string json)
        {
            TypeId = typeId;
            Json = json;
        }

        public string TypeId { get; set; }
        public string Json { get; set; }
    }
}
