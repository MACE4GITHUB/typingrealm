using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Messaging.Serialization;

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

        // TODO: Make sure protobuf works over multiple servers with new implementation.
        // This is a temporary hack so that protobuf works with multiple servers
        // when Json is enabled. MessageData will always be the first message,
        // hence protobuf field number for it will always be "1" for any servers.
        // TODO: Make sure we really need this and protobuf will be used to basically serialize a single message over network.
        var messageData = types.SingleOrDefault(type => type == typeof(MessageData));
        if (messageData != null)
        {
            types.Remove(messageData);

            // TODO: Consider adding MessageData always, even if it wasn't passed here.
            types = new[] { typeof(MessageData) }.Concat(types).ToList();
        }

        return types
            .Select((type, index) => new
            {
                    // TODO: Unit test this exception or change implementation.
                    TypeId = type.FullName ?? throw new InvalidOperationException("Type doesn't have name."),
                Type = type
            })
            .ToDictionary(x => x.TypeId, x => x.Type);
    }
}
