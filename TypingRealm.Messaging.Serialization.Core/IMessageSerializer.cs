using System;

namespace TypingRealm.Messaging.Serialization
{
    public interface IMessageSerializer
    {
        string Serialize(object instance);
        object Deserialize(string data, Type messageType);
    }
}
