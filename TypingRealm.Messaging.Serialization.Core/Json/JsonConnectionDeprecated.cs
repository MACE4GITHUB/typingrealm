using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Serialization.Json
{
    /// <summary>
    /// This message serializes and wraps message into
    /// <see cref="JsonSerializedMessage.Json"/> property and assigns corresponding
    /// type id from <see cref="IMessageTypeCache"/>.
    /// </summary>
    public sealed class JsonConnectionDeprecated : IConnection
    {
        private readonly IConnection _connection;
        private readonly IMessageTypeCache _messageTypes;
        private readonly JsonSerializerOptions _options;

        public JsonConnectionDeprecated(
            IConnection connection,
            IMessageTypeCache messageTypes)
        {
            _connection = connection;
            _messageTypes = messageTypes;
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _options.Converters.Add(new JsonStringEnumConverter());
        }

        public ValueTask SendAsync(object message, CancellationToken cancellationToken)
        {
            var typeId = _messageTypes.GetTypeId(message.GetType());
            // TODO: Put logging here and investigate. It silently fails when serialization fails.
            var json = JsonSerializer.Serialize(message, _options);
            var jsonSerializedMessage = new JsonSerializedMessage(typeId, json);

            return _connection.SendAsync(jsonSerializedMessage, cancellationToken);
        }

        public async ValueTask<object> ReceiveAsync(CancellationToken cancellationToken)
        {
            var message = (JsonSerializedMessage)await _connection
                .ReceiveAsync(cancellationToken)
                .ConfigureAwait(false);

            var type = _messageTypes.GetTypeById(message.TypeId);
            // TODO: Put logging here and investigate. It silently fails when deserialization fails.
            var deserialized = JsonSerializer.Deserialize(message.Json, type, _options);
            if (deserialized == null)
                throw new InvalidOperationException($"Could not deserialize an object of type {type.Name}.");

            return deserialized;
        }
    }
}
