using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace TypingRealm.Typing.Framework
{
    public sealed class JsonFileRepository<T> : IRepository<T>
        where T : IIdentifiable
    {
        private readonly string _filePath;
        private readonly object _lock = new object();

        public JsonFileRepository(string filePath)
        {
            _filePath = filePath;
        }

        public ValueTask<T?> FindAsync(string key)
        {
            lock (_lock)
            {
                if (!File.Exists(_filePath))
                    File.WriteAllText(_filePath, "[]");

                var content = File.ReadAllText(_filePath);
                var deserialized = JsonSerializer.Deserialize<IEnumerable<T>>(content);
                if (deserialized == null)
                    throw new InvalidOperationException("Could not deserialize data from the json file.");

                return new ValueTask<T?>(deserialized.FirstOrDefault(x => x.Id == key));
            }
        }

        public ValueTask<string> NextIdAsync()
        {
            return new ValueTask<string>(Guid.NewGuid().ToString());
        }

        public ValueTask SaveAsync(T entity)
        {
            lock (_lock)
            {
                if (!File.Exists(_filePath))
                    File.WriteAllText(_filePath, "[]");

                var content = File.ReadAllText(_filePath);
                var deserialized = JsonSerializer.Deserialize<IEnumerable<T>>(content);
                if (deserialized == null)
                    throw new InvalidOperationException("Could not deserialize data from the json file.");

                var list = deserialized.ToList();
                var existing = list.FirstOrDefault(x => x.Id == entity.Id);
                if (existing != null)
                {
                    list.Remove(existing);
                }

                list.Add(entity);

                var serialized = JsonSerializer.Serialize(list);
                File.WriteAllText(_filePath, serialized);
                return default;
            }
        }
    }
}
