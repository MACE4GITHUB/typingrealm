using System.IO;

namespace TypingRealm.Messaging.Serialization.Protobuf
{
    public sealed class ProtobufConnectionFactory : IProtobufConnectionFactory
    {
        private readonly IMessageTypeCache _messageTypes;

        public ProtobufConnectionFactory(IMessageTypeCache messageTypes)
        {
            _messageTypes = messageTypes;
        }

        public ProtobufConnection CreateProtobufConnection(Stream stream)
        {
            return new ProtobufConnection(stream, _messageTypes);
        }
    }
}
