using System;

namespace TypingRealm.Messaging.Serialization.Protobuf;

/// <summary>
/// Additional layer between <see cref="IMessageTypeCache"/> and Protobuf FieldNumber.
/// </summary>
public interface IProtobufFieldNumberCache
{
    Type GetTypeByFieldNumber(int fieldNumber);
    int GetFieldNumber(Type type);
}
