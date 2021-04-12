using System.IO;

namespace TypingRealm.Messaging.Serialization.Protobuf
{
    public sealed class Program
    {
        public static void Main()
        {
        }
    }

    public sealed class ProtobufConnectionFactory : IProtobufConnectionFactory
    {
        private readonly IProtobufFieldNumberCache _fieldNumberCache;
        private readonly IProtobufStreamSerializer _protobuf;

        private readonly IMessageSerializer _messageSerializer;
        private readonly IMessageTypeCache _messageTypeCache;
        private readonly IClientToServerMessageMetadataFactory _clientToServerMessageMetadataFactory;

        public ProtobufConnectionFactory(
            IProtobufFieldNumberCache fieldNumberCache,
            IProtobufStreamSerializer protobuf,
            IMessageSerializer messageSerializer,
            IMessageTypeCache messageTypeCache,
            IClientToServerMessageMetadataFactory clientToServerMessageMetadataFactory)
        {
            _fieldNumberCache = fieldNumberCache;
            _protobuf = protobuf;

            _messageSerializer = messageSerializer;
            _messageTypeCache = messageTypeCache;
            _clientToServerMessageMetadataFactory = clientToServerMessageMetadataFactory;
        }

        // TODO: Unit test this (possibly after we move ForClient / ForServer from here somehow).
        public IConnection CreateProtobufConnectionForClient(Stream stream)
        {
            return new ProtobufConnection(stream, _fieldNumberCache, _protobuf)
                .ForClient(_messageSerializer, _messageTypeCache, _clientToServerMessageMetadataFactory);
        }

        public IConnection CreateProtobufConnectionForServer(Stream stream)
        {
            return new ProtobufConnection(stream, _fieldNumberCache, _protobuf)
                .ForServer(_messageSerializer, _messageTypeCache);
        }
    }
}
