using System.IO;

namespace TypingRealm.Messaging.Serialization.Protobuf;

public interface IProtobufConnectionFactory
{
    IConnection CreateProtobufConnection(Stream stream);
}

public sealed class ProtobufConnectionFactory : IProtobufConnectionFactory
{
    private readonly IProtobufFieldNumberCache _fieldNumberCache;
    private readonly IProtobufStreamSerializer _protobuf;
    private readonly IMessageSerializer _messageSerializer;
    private readonly IMessageTypeCache _messageTypeCache;
    private readonly IMessageMetadataFactory _messageMetadataFactory;

    public ProtobufConnectionFactory(
        IProtobufFieldNumberCache fieldNumberCache,
        IProtobufStreamSerializer protobuf,
        IMessageSerializer messageSerializer,
        IMessageTypeCache messageTypeCache,
        IMessageMetadataFactory messageMetadataFactory)
    {
        _fieldNumberCache = fieldNumberCache;
        _protobuf = protobuf;

        _messageSerializer = messageSerializer;
        _messageTypeCache = messageTypeCache;
        _messageMetadataFactory = messageMetadataFactory;
    }

    public IConnection CreateProtobufConnection(Stream stream)
    {
        return new ProtobufConnection(stream, _fieldNumberCache, _protobuf)
            .AddCoreMessageSerialization(_messageSerializer, _messageTypeCache, _messageMetadataFactory);
    }
}
