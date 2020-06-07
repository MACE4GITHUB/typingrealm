using System;
using System.Collections.Generic;
using TypingRealm.Domain.Movement;
using TypingRealm.Messaging.Connecting;

namespace TypingRealm.Domain.Infrastructure
{
    public sealed class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly Dictionary<PlayerId, Player> _playerIdToPlayer
            = new Dictionary<PlayerId, Player>();

        public InMemoryPlayerRepository(
            IConnectedClientStore connectedClients,
            ILocationStore locationStore,
            IRoadStore roadStore)
        {
            _playerIdToPlayer.Add(new PlayerId("ivan-id"), new Player(
                new PlayerId("ivan-id"),
                new PlayerName("ivan"),
                new LocationId("village"),
                locationStore,
                roadStore,
                null,
                group => connectedClients.Find("ivan-id")!.Group = group));

            _playerIdToPlayer.Add(new PlayerId("john-id"), new Player(
                new PlayerId("john-id"),
                new PlayerName("john"),
                new LocationId("village"),
                locationStore,
                roadStore,
                null,
                group => connectedClients.Find("john-id")!.Group = group));
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
