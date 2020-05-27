using System.IO;

namespace TypingRealm.Messaging.Serialization.Protobuf
{
    public sealed class ProtobufConnectionFactory : IProtobufConnectionFactory
    {
        private readonly IMessageTypeCache _messageTypes;
        private readonly IProtobuf _protobuf;

        public ProtobufConnectionFactory(IMessageTypeCache messageTypes, IProtobuf protobuf)
        {
            _messageTypes = messageTypes;
            _protobuf = protobuf;
        }

        public ProtobufConnection CreateProtobufConnection(Stream stream)
        {
            return new ProtobufConnection(stream, _messageTypes, _protobuf);
        }
    }
}
