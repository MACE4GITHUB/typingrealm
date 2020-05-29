using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Serialization.Json
{
    /// <summary>
    /// This message serializes and wraps message into
    /// <see cref="JsonSerializedMessage.Json"/> property and assigns corresponding
    /// type id from <see cref="IMessageTypeCache"/>.
    /// </summary>
    public sealed class JsonConnection : IConnection
    {
        private readonly IConnection _connection;
        private readonly IMessageTypeCache _messageTypes;

        public JsonConnection(
            IConnection connection,
            IMessageTypeCache messageTypes)
        {
            _connection = connection;
            _messageTypes = messageTypes;
        }

        public ValueTask SendAsync(object message, CancellationToken cancellationToken)
        {
            var typeId = _messageTypes.GetTypeId(message.GetType());
            var json = JsonSerializer.Serialize(message);
            var jsonSerializedMessage = new JsonSerializedMessage(typeId, json);

            return _connection.SendAsync(jsonSerializedMessage, cancellationToken);
        }

        public async ValueTask<object> ReceiveAsync(CancellationToken cancellationToken)
        {
            var message = (JsonSerializedMessage)await _connection
                .ReceiveAsync(cancellationToken)
                .ConfigureAwait(false);

            var type = _messageTypes.GetTypeById(message.TypeId);
            return JsonSerializer.Deserialize(message.Json, type);
        }
    }
}
