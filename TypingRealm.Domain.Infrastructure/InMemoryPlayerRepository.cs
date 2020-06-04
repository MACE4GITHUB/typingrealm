using System;
using System.Collections.Generic;
using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain.Infrastructure
{
    public sealed class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly Dictionary<PlayerId, Player> _playerIdToPlayer
            = new Dictionary<PlayerId, Player>();

        public InMemoryPlayerRepository(IPlayerFactory playerFactory)
        {
            _playerIdToPlayer.Add(new PlayerId("ivan-id"), playerFactory.Create(
                new PlayerId("ivan-id"),
                new PlayerName("ivan"),
                new LocationId("village")));

            _playerIdToPlayer.Add(new PlayerId("john-id"), playerFactory.Create(
                new PlayerId("john-id"),
                new PlayerName("john"),
                new LocationId("village")));
        }

        public Player? Find(PlayerId playerId)
        {
            if (!_playerIdToPlayer.ContainsKey(playerId))
                return null;

            return _playerIdToPlayer[playerId];
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

        public PlayerId NextId()
        {
            return new PlayerId(Guid.NewGuid().ToString());
        }
    }
}
