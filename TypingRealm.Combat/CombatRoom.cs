using System.Collections.Generic;

namespace TypingRealm.Combat
{
    public sealed class CombatRoom
    {
        public CombatRoom(List<Player> players, string combatRoomId)
        {
            Players = players;
            CombatRoomId = combatRoomId;
        }

        public List<Player> Players { get; set; }
        public string CombatRoomId { get; set; }
    }
}
