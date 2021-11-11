using System.Threading.Tasks;

namespace TypingRealm.Typing.Framework
{
    public interface IRepository<T>
    {
        ValueTask<T?> FindAsync(string key);
        ValueTask SaveAsync(T entity);

        ValueTask<string> NextIdAsync();
    }
}
