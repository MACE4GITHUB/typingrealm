using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Typing.Framework;

public interface IRepository<T>
{
    ValueTask<T?> FindAsync(string key);
    ValueTask SaveAsync(T entity);
    ValueTask<string> NextIdAsync();

    IAsyncEnumerable<T> LoadAllAsync(Func<T, bool> predicate);
}
