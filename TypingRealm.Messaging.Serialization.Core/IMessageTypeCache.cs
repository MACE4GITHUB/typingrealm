using System;
using System.Collections.Generic;

namespace TypingRealm.Messaging.Serialization
{
    /// <summary>
    /// The cache stores mapping between message type and its string identity.
    /// </summary>
    public interface IMessageTypeCache
    {
        Type GetTypeById(string typeId);
        string GetTypeId(Type type);
        IEnumerable<KeyValuePair<string, Type>> GetAllTypes();
    }
}
