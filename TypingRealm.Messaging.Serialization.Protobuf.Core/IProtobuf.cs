using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;
using ProtoBuf.Meta;

namespace TypingRealm.Messaging.Serialization.Protobuf
{
    public interface IProtobuf
    {
        object Deserialize(Stream source, Func<int, Type> typeResolver);
        void Serialize(Stream destination, object instance, int fieldNumber);
    }

    // Not tested.
    public sealed class Protobuf : IProtobuf
    {
        public Protobuf(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                AddTypeToRuntimeTypeModel(type);
            }
        }

        public object Deserialize(Stream source, Func<int, Type> typeResolver)
        {
            if (Serializer.NonGeneric.TryDeserializeWithLengthPrefix(
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
            Serializer.NonGeneric.SerializeWithLengthPrefix(
                destination, instance, PrefixStyle.Base128, fieldNumber);
        }

        private void AddTypeToRuntimeTypeModel(Type type)
        {
            RuntimeTypeModel.Default.Add(type, false)
                .Add(type
                    .GetProperties()
                    .Select(property => property.Name)
                    .OrderBy(name => name)
                    .ToArray());
        }
    }
}
