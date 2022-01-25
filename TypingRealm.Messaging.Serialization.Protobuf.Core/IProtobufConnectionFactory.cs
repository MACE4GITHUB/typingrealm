using System.IO;

namespace TypingRealm.Messaging.Serialization.Protobuf;

public interface IProtobufConnectionFactory
{
    IConnection CreateProtobufConnectionForClient(Stream stream);
    IConnection CreateProtobufConnectionForServer(Stream stream);
}
