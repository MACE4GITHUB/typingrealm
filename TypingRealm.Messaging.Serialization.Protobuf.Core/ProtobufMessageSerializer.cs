using System;
using System.Collections.Generic;
using System.IO;

namespace TypingRealm.Messaging.Serialization.Protobuf;

// TODO: Unit test this class. Possibly together with ProtobufStreamSerializer.
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
