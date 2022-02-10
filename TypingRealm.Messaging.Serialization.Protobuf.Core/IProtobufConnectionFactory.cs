using System.IO;

namespace TypingRealm.Messaging.Serialization.Protobuf;

public interface IProtobufConnectionFactory
{
    IConnection CreateProtobufConnection(Stream stream);
}
