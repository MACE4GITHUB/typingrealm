using System.IO;

namespace TypingRealm.Messaging.Serialization.Protobuf;

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
    private readonly IMessageMetadataFactory _clientToServerMessageMetadataFactory;

    public ProtobufConnectionFactory(
        IProtobufFieldNumberCache fieldNumberCache,
        IProtobufStreamSerializer protobuf,
        IMessageSerializer messageSerializer,
        IMessageTypeCache messageTypeCache,
        IMessageMetadataFactory clientToServerMessageMetadataFactory)
    {
        _fieldNumberCache = fieldNumberCache;
        _protobuf = protobuf;

        _messageSerializer = messageSerializer;
        _messageTypeCache = messageTypeCache;
        _clientToServerMessageMetadataFactory = clientToServerMessageMetadataFactory;
    }

    // TODO: Unit test this (possibly after we move ForClient / ForServer from here somehow).
    public IConnection CreateProtobufConnection(Stream stream)
    {
        return new ProtobufConnection(stream, _fieldNumberCache, _protobuf)
            .AddCoreMessageSerialization(_messageSerializer, _messageTypeCache, _clientToServerMessageMetadataFactory);
    }
}
