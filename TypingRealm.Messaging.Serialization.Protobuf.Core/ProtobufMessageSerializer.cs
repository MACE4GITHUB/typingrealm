using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf.Meta;

namespace TypingRealm.Messaging.Serialization.Protobuf
{
    public sealed class ProtobufMessageSerializer : IMessageSerializer
    {
        private readonly RuntimeTypeModel _model;

        public ProtobufMessageSerializer(IEnumerable<Type> types)
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

        public object Deserialize(string data, Type messageType)
        {
            ReadOnlySpan<byte> span = Convert.FromBase64String(data);

            return _model.Deserialize(messageType, span);
        }

        public string Serialize(object instance)
        {
            using var stream = new MemoryStream();

            _model.Serialize(stream, instance);

            // TODO: Consider using ReadOnlySpan for better performance (bytes instead of string).
            var serialized = Convert.ToBase64String(stream.ToArray());

            return serialized;
        }
    }
}
