using System;
using System.Text.Json;

namespace TypingRealm.Messaging.Serialization.Json
{
    public sealed class JsonMessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonMessageSerializer(JsonSerializerOptions options)
        {
            _options = options;
        }

        public object Deserialize(string data, Type messageType)
        {
            return JsonSerializer.Deserialize(data, messageType, _options)
                ?? throw new InvalidOperationException($"Could not deserialize message of type {messageType.Name}.");
        }

        public string Serialize(object instance)
        {
            return JsonSerializer.Serialize(instance, _options);
        }
    }
}
