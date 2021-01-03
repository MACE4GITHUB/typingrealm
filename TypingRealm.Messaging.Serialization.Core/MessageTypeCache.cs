using System;
using System.Collections.Generic;
using System.Linq;
using TypingRealm.Messaging.Serialization.Json;

namespace TypingRealm.Messaging.Serialization
{
    /// <summary>
    /// The cache stores mapping between message type and its identity.
    /// </summary>
    public sealed class MessageTypeCache : IMessageTypeCache
    {
        private readonly Dictionary<string, Type> _idToType;
        private readonly Dictionary<Type, string> _typeToId;

        public MessageTypeCache(IEnumerable<Type> messageTypes)
        {
            _idToType = ToDictionaryById(messageTypes);
            _typeToId = _idToType.ToDictionary(x => x.Value, x => x.Key);
        }

        public IEnumerable<KeyValuePair<string, Type>> GetAllTypes()
        {
            return _idToType;
        }

        public Type GetTypeById(string typeId)
        {
            if (!_idToType.ContainsKey(typeId))
                throw new InvalidOperationException($"Type with id {typeId} doesn't exist in {nameof(MessageTypeCache)}.");

            return _idToType[typeId];
        }

        public string GetTypeId(Type type)
        {
            if (!_typeToId.ContainsKey(type))
                throw new InvalidOperationException($"Type {type.FullName} doesn't exist in {nameof(MessageTypeCache)}.");

            return _typeToId[type];
        }

        private static Dictionary<string, Type> ToDictionaryById(IEnumerable<Type> messageTypes)
        {
            var types = messageTypes
                .OrderBy(type => type.FullName)
                .ToList();

            // This is a temporary hack so that protobuf works with multiple servers
            // when Json is enabled. JsonSerializedMessage will always be the first message,
            // hence protobuf field number for it will always be "1" for any servers.
            var jsonSerializedMessage = types.SingleOrDefault(type => type == typeof(JsonSerializedMessage));
            if (jsonSerializedMessage != null)
            {
                types.Remove(jsonSerializedMessage);
                types = new[] { jsonSerializedMessage }.Concat(types).ToList();
            }

            return types
                .Select((type, index) => new
                {
                    TypeId = type.FullName,
                    Type = type
                })
                .ToDictionary(x => x.TypeId, x => x.Type);
        }
    }
}
