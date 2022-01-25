namespace TypingRealm.Messaging.Serialization.Json;

/// <summary>
/// Wrapper message around serialized message.
/// </summary>
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

    /// <summary>
    /// Type identity, should be set from <see cref="IMessageTypeCache"/>
    /// for proper deserialization.
    /// </summary>
    public string TypeId { get; set; }

    /// <summary>
    /// Serialized message json content.
    /// </summary>
    public string Json { get; set; }
}
