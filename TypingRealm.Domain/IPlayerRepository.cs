using System.Collections.Generic;

namespace TypingRealm.Domain
{
    public interface IPlayerRepository
    {
        Player FindByClientId(string clientId);
        Player FindByPlayerId(PlayerId playerId);
        void Save(string clientId, Player player);

        public IEnumerable<Player> FindPlayersVisibleTo(PlayerId playerId);
    }
}
