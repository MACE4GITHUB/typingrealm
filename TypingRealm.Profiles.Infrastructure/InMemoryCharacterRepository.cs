using System.Collections.Generic;
using System.Linq;
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

    public sealed class InMemoryActivityRepository : IActivityRepository
    {
        internal readonly Dictionary<string, Activity> _cache
            = new Dictionary<string, Activity>();

        public Activity? Find(string activityId)
        {
            if (!_cache.ContainsKey(activityId))
                return null;

            return _cache[activityId];
        }

        public Activity? FindActiveActivityForCharacter(string characterId)
        {
            return _cache.Values.SingleOrDefault(activity
                => !activity.IsFinished // Get only active activities.
                && activity.CharacterIds.Contains(characterId));
        }

        public void Save(Activity activity)
        {
            if (!_cache.ContainsKey(activity.ActivityId))
            {
                _cache.Add(activity.ActivityId, activity);
            }

            _cache[activity.ActivityId] = activity;
        }

        public bool ValidateCharactersDoNotHaveActiveActivity(IEnumerable<string> characterIds)
        {
            return !_cache.Values
                .Where(x => !x.IsFinished) // Get only active activities.
                .SelectMany(activity => activity.CharacterIds)
                .Intersect(characterIds)
                .Any();
        }
    }
}
