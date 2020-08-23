using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Combat
{
    public sealed class CombatRoomStore : ICombatRoomStore
    {
        private readonly Dictionary<string, CombatRoom> _cache
            = new Dictionary<string, CombatRoom>();

        public CombatRoom FindInBattle(string playerId)
        {
            return _cache.Values.FirstOrDefault(x => x.Players.Any(p => p.PlayerId == playerId));
        }

        public CombatRoom FindOrCreate(string combatRoomId, string playerId)
        {
            if (!_cache.ContainsKey(combatRoomId))
                _cache.Add(combatRoomId, new CombatRoom(new List<Player>(), combatRoomId));

            if (!_cache[combatRoomId].Players.Any(p => p.PlayerId == playerId))
                _cache[combatRoomId].Players.Add(new Player(playerId, 100, 100));

            return _cache[combatRoomId];
        }
    }
}
