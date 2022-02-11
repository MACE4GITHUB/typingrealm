using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace TypingRealm.Messaging.Serialization.Protobuf;

/// <summary>
/// Wrapper around Protobuf implementation.
/// </summary>
public interface IProtobufStreamSerializer
{
    object Deserialize(Stream source, Func<int, Type> typeResolver);
    void Serialize(Stream destination, object instance, int fieldNumber);
}

// TODO: Unit test this class. Possibly together with ProtobufMessageSerializer.
public sealed class ProtobufStreamSerializer : ProtobufRuntimeModelSerializer, IProtobufStreamSerializer
{
    public ProtobufStreamSerializer(IEnumerable<Type> types) : base(types) { }

    public object Deserialize(Stream source, Func<int, Type> typeResolver)
    {
        if (TryDeserializeWithLengthPrefix(
            source,
            PrefixStyle.Base128,
            fieldNumber => typeResolver(fieldNumber),
            out var value))
        {
            return value;
        }

        throw new InvalidOperationException("Could not deserialize Protobuf value from stream.");
    }

    public void Serialize(Stream destination, object instance, int fieldNumber)
    {
        if (instance is null)
            throw new ArgumentNullException(nameof(instance));

        Model.SerializeWithLengthPrefix(
            destination, instance, instance.GetType(), PrefixStyle.Base128, fieldNumber);
    }

    private bool TryDeserializeWithLengthPrefix(Stream source, PrefixStyle style, ProtoBuf.TypeResolver resolver, out object value)
    {
        value = Model.DeserializeWithLengthPrefix(source, null, null, style, 0, resolver);
        return value is object;
    }
}
