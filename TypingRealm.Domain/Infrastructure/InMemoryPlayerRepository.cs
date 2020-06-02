using System;
using System.Collections.Generic;

namespace TypingRealm.Domain.Infrastructure
{
    public sealed class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly Dictionary<PlayerId, Player> _playerIdToPlayer
            = new Dictionary<PlayerId, Player>();

        public Player? Find(PlayerId playerId)
        {
            if (!_playerIdToPlayer.ContainsKey(playerId))
                return null;

            return _playerIdToPlayer[playerId];
        }

        public IEnumerable<Player> FindPlayersVisibleTo(PlayerId playerId)
        {
            var player = _playerIdToPlayer[playerId];
            var uniqueLocation = player.GetUniquePlayerPosition();

            foreach (var visiblePlayer in _playerIdToPlayer.Values)
            {
                if (visiblePlayer.GetUniquePlayerPosition() == uniqueLocation)
                    yield return visiblePlayer;
            }
        }

        public PlayerId NextId()
        {
            return new PlayerId(Guid.NewGuid().ToString());
        }

        public void Save(Player player)
        {
            if (_playerIdToPlayer.ContainsKey(player.PlayerId))
            {
                _playerIdToPlayer[player.PlayerId] = player;
                return;
            }

            _playerIdToPlayer.Add(player.PlayerId, player);
        }
    }
}
