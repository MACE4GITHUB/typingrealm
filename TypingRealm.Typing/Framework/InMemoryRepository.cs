using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Typing.Framework
{
    public class InMemoryRepository<T> : IRepository<T>
        where T : IIdentifiable
    {
        private readonly Dictionary<string, T> _entities
            = new Dictionary<string, T>();

        public ValueTask<T?> FindAsync(string key)
        {
            if (!_entities.ContainsKey(key))
                return new ValueTask<T?>((T?)default);

            return new ValueTask<T?>(_entities[key]);
        }

        public ValueTask SaveAsync(T entity)
        {
            if (_entities.ContainsKey(entity.Id))
                _entities[entity.Id] = entity;
            else
                _entities.Add(entity.Id, entity);

            return default;
        }

        public ValueTask<string> NextIdAsync()
        {
            return new ValueTask<string>(Guid.NewGuid().ToString());
        }
    }
}
