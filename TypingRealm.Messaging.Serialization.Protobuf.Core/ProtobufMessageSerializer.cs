using System;
using System.Collections.Generic;
using System.IO;

namespace TypingRealm.Messaging.Serialization.Protobuf;

// TODO: Implement nested types serialization even when they are not marked by Message attribute.
// Unit test this new feature (uncomment related test code and unit test nested types).
public sealed class ProtobufMessageSerializer : ProtobufRuntimeModelSerializer, IMessageSerializer
{
    public ProtobufMessageSerializer(IEnumerable<Type> types) : base(types) { }

    public object Deserialize(string data, Type messageType)
    {
        ReadOnlySpan<byte> span = Convert.FromBase64String(data);

        return Model.Deserialize(messageType, span);
    }

    public string Serialize(object instance)
    {
        using var stream = new MemoryStream();

        Model.Serialize(stream, instance);

        // TODO: Consider using ReadOnlySpan for better performance (bytes instead of string).
        var serialized = Convert.ToBase64String(stream.ToArray());

        return serialized;
    }
}
