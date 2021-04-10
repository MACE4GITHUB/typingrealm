using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Serialization.Protobuf
{
    /// <summary>
    /// Serializes messages using Protobuf protocol and sends them over stream.
    /// </summary>
    public sealed class ProtobufConnection : IConnection
    {
        private readonly Stream _stream;
        private readonly IProtobufFieldNumberCache _fieldNumberCache;
        private readonly IProtobuf _protobuf;

        public ProtobufConnection(
            Stream stream,
            IProtobufFieldNumberCache fieldNumberCache,
            IProtobuf protobuf)
        {
            _stream = stream;
            _fieldNumberCache = fieldNumberCache;
            _protobuf = protobuf;
        }

        public async ValueTask<object> ReceiveAsync(CancellationToken cancellationToken)
        {
            // Use trick to wait asynchronously until there is something in the stream.
            // This will throw OperationCanceledException if cancellation token signals for cancellation.
            await _stream.ReadAsync(Array.Empty<byte>(), 0, 0, cancellationToken).ConfigureAwait(false);

            // TODO: Improve implementation to not (potentially) block the thread on I/O operation.
            // Buffer the stream asynchronously until Base128 final marker is met.

            return _protobuf.Deserialize(
                _stream,
                fieldNumber => _fieldNumberCache.GetTypeByFieldNumber(fieldNumber));
        }

        public async ValueTask SendAsync(object message, CancellationToken cancellationToken)
        {
            using var memoryStream = new MemoryStream();

            var fieldNumber = Convert.ToInt32(
                _fieldNumberCache.GetFieldNumber(message.GetType()));

            _protobuf.Serialize(memoryStream, message, fieldNumber);

            await _stream.WriteAsync(memoryStream.GetBuffer(), 0, (int)memoryStream.Position, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
