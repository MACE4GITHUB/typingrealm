using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace TypingRealm.Messaging.Serialization.Protobuf
{
    /// <summary>
    /// Serializes messages using Protobuf protocol and sends them over stream.
    /// </summary>
    public sealed class ProtobufConnection : IConnection
    {
        private readonly Stream _stream;
        private readonly IMessageTypeCache _messageTypes;

        public ProtobufConnection(Stream stream, IMessageTypeCache messageTypes)
        {
            _stream = stream;
            _messageTypes = messageTypes;
        }

        public async ValueTask<object> ReceiveAsync(CancellationToken cancellationToken)
        {
            // Use trick to wait asynchronously until there is something in the stream.
            // This will throw OperationCanceledException if cancellation token signals for cancellation.
            await _stream.ReadAsync(Array.Empty<byte>(), 0, 0, cancellationToken).ConfigureAwait(false);

            if (Serializer.NonGeneric.TryDeserializeWithLengthPrefix(
                _stream,
                PrefixStyle.Base128,
                fieldNumber => _messageTypes.GetTypeById(fieldNumber.ToString()),
                out var message))
            {
                return message;
            }

            // We can get here if client invalidly disconnected.
            throw new InvalidOperationException("Could not deserialize message from stream.");
        }

        public async ValueTask SendAsync(object message, CancellationToken cancellationToken)
        {
            using var memoryStream = new MemoryStream();

            var fieldNumber = Convert.ToInt32(
                _messageTypes.GetTypeId(message.GetType()));

            Serializer.NonGeneric.SerializeWithLengthPrefix(
                memoryStream, message, PrefixStyle.Base128, fieldNumber);

            await _stream.WriteAsync(memoryStream.GetBuffer(), 0, (int)memoryStream.Position, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
