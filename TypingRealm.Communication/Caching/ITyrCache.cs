using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Communication;

public interface ITyrCache
{
    ValueTask<T?> GetValueAsync<T>(string key);
    ValueTask SetValueAsync<T>(string key, T value, TimeSpan? expiration = null);
    ValueTask MergeCollectionAsync<T>(string key, IEnumerable<T> collection, bool isUnique = true);
}
