using System;
using TypingRealm.Serialization;

namespace TypingRealm.Messaging.Serialization.Json;

public sealed class JsonMessageSerializer : IMessageSerializer
{
    private readonly ISerializer _serializer;

    public JsonMessageSerializer(ISerializer serializer)
    {
        _serializer = serializer;
    }

    public object Deserialize(string data, Type messageType)
    {
        return _serializer.Deserialize(data, messageType)
            ?? throw new InvalidOperationException($"Could not deserialize message of type {messageType.Name}.");
    }

    public string Serialize(object instance)
    {
        return _serializer.Serialize(instance);
    }
}
