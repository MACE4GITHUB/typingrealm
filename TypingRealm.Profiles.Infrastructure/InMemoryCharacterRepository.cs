using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Profiles.Infrastructure
{
    public sealed class InMemoryCharacterRepository : ICharacterRepository
    {
        internal readonly Dictionary<CharacterId, Character> _cache
            = new Dictionary<CharacterId, Character>();

        public ValueTask<Character?> FindAsync(CharacterId characterId)
        {
            _cache.TryGetValue(characterId, out var character);
            return new ValueTask<Character?>(character);
        }

        public ValueTask<CharacterId> NextIdAsync()
        {
            return new ValueTask<CharacterId>(CharacterId.New());
        }

        public ValueTask SaveAsync(Character character)
        {
            if (!_cache.ContainsKey(character.CharacterId))
            {
                _cache.Add(character.CharacterId, character);
                return default;
            }

            _cache[character.CharacterId] = character;
            return default;
        }
    }
}
