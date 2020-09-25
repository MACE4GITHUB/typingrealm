using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Messaging.Serialization.Protobuf
{
    public sealed class ProtobufFieldNumberCache : IProtobufFieldNumberCache
    {
        private readonly Dictionary<int, Type> _fieldNumberToType;
        private readonly Dictionary<Type, int> _typeToFieldNumber;

        public ProtobufFieldNumberCache(IMessageTypeCache messageTypes)
        {
            _fieldNumberToType = messageTypes.GetAllTypes()
                .Select((type, index) => new
                {
                    FieldNumber = index + 1,
                    Type = type.Value
                })
                .ToDictionary(x => x.FieldNumber, x => x.Type);

            _typeToFieldNumber = _fieldNumberToType.ToDictionary(x => x.Value, x => x.Key);
        }

        public int GetFieldNumber(Type type)
        {
            return _typeToFieldNumber[type];
        }

        public Type GetTypeByFieldNumber(int fieldNumber)
        {
            return _fieldNumberToType[fieldNumber];
        }
    }
}
