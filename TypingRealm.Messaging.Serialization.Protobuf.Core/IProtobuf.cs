using System;
using System.IO;

namespace TypingRealm.Messaging.Serialization.Protobuf
{
    /// <summary>
    /// Wrapper around Protobuf implementation, typically static Serializer class.
    /// </summary>
    public interface IProtobuf
    {
        object Deserialize(Stream source, Func<int, Type> typeResolver);
        void Serialize(Stream destination, object instance, int fieldNumber);
    }
}
