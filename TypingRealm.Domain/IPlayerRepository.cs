using System.Collections.Generic;

namespace TypingRealm.Domain
{
    public interface IPlayerRepository
    {
        Player GetByClientId(string clientId);
        Player GetByPlayerId(PlayerId playerId);
        void Save(string clientId, Player player);

        public IEnumerable<Player> GetPlayersVisibleTo(PlayerId playerId);
    }
}
