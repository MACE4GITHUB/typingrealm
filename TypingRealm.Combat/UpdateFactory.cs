using System.Collections.Generic;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Combat
{
    [Message]
    public sealed class Update
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public List<Player> Players { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }

    public sealed class UpdateFactory : IUpdateFactory
    {
        private readonly ICombatRoomStore _combatRooms;

        public UpdateFactory(ICombatRoomStore combatRooms)
        {
            _combatRooms = combatRooms;
        }

        public object GetUpdateFor(string clientId)
        {
            return new Update
            {
                Players = _combatRooms.FindInBattle(clientId).Players
            };
        }
    }

}
