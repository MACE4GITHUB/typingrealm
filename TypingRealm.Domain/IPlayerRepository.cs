using System.Collections.Generic;

namespace TypingRealm.Domain
{
    public interface IPlayerRepository
    {
        Player? Find(PlayerId playerId);
        void Save(Player player);
        IEnumerable<Player> FindPlayersVisibleTo(PlayerId playerId);
        PlayerId NextId();
    }
}
