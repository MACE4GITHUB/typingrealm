using System.IO;

namespace TypingRealm.Messaging.Serialization.Protobuf
{
    public interface IProtobufConnectionFactory
    {
        ProtobufConnection CreateProtobufConnection(Stream stream);
    }
}
