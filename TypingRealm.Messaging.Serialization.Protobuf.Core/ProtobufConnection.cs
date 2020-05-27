﻿using System;
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
        private readonly IMessageTypeCache _messageTypes;
        private readonly IProtobuf _protobuf;

        public ProtobufConnection(
            Stream stream,
            IMessageTypeCache messageTypes,
            IProtobuf protobuf)
        {
            _stream = stream;
            _messageTypes = messageTypes;
            _protobuf = protobuf;
        }

        public async ValueTask<object> ReceiveAsync(CancellationToken cancellationToken)
        {
            // Use trick to wait asynchronously until there is something in the stream.
            // This will throw OperationCanceledException if cancellation token signals for cancellation.
            await _stream.ReadAsync(Array.Empty<byte>(), 0, 0, cancellationToken).ConfigureAwait(false);

            return _protobuf.Deserialize(
                _stream,
                fieldNumber => _messageTypes.GetTypeById(fieldNumber.ToString()));
        }

        public async ValueTask SendAsync(object message, CancellationToken cancellationToken)
        {
            using var memoryStream = new MemoryStream();

            var fieldNumber = Convert.ToInt32(
                _messageTypes.GetTypeId(message.GetType()));

            _protobuf.Serialize(memoryStream, message, fieldNumber);

            await _stream.WriteAsync(memoryStream.GetBuffer(), 0, (int)memoryStream.Position, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
