using System.IO;

namespace TypingRealm.Messaging.Serialization.Protobuf
{
    public sealed class ProtobufConnectionFactory : IProtobufConnectionFactory
    {
        private readonly IProtobufFieldNumberCache _fieldNumberCache;
        private readonly IProtobuf _protobuf;

        public ProtobufConnectionFactory(IProtobufFieldNumberCache fieldNumberCache, IProtobuf protobuf)
        {
            _fieldNumberCache = fieldNumberCache;
            _protobuf = protobuf;
        }

        public ProtobufConnection CreateProtobufConnection(Stream stream)
        {
            return new ProtobufConnection(stream, _fieldNumberCache, _protobuf);
        }
    }
}
