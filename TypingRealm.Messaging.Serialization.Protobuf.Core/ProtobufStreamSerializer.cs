using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;
using ProtoBuf.Meta;

namespace TypingRealm.Messaging.Serialization.Protobuf
{
    // TODO: Unit test this class. Possibly together with ProtobufMessageSerializer.
    public sealed class ProtobufStreamSerializer : IProtobufStreamSerializer
    {
        private readonly RuntimeTypeModel _model;

        public ProtobufStreamSerializer(IEnumerable<Type> types)
        {
            _model = RuntimeTypeModel.Create();

            foreach (var type in types)
            {
                _model.Add(type, false)
                    .Add(type
                        .GetProperties()
                        .Select(property => property.Name)
                        .OrderBy(name => name)
                        .ToArray());
            }

            // TODO: uncomment this for better performance.
            //_model.Compile();
        }

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

            _model.SerializeWithLengthPrefix(
                destination, instance, instance.GetType(), PrefixStyle.Base128, fieldNumber);
        }

        private bool TryDeserializeWithLengthPrefix(Stream source, PrefixStyle style, ProtoBuf.TypeResolver resolver, out object value)
        {
            value = _model.DeserializeWithLengthPrefix(source, null, null, style, 0, resolver);
            return value is object;
        }
    }
}
