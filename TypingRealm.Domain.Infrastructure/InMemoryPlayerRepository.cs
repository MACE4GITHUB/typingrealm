using System.Collections.Generic;
using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain.Infrastructure
{
    public sealed class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly IPlayerPersistenceFactory _playerPersistenceFactory;
        private readonly Dictionary<PlayerId, PlayerState> _playerIdToPlayer
            = new Dictionary<PlayerId, PlayerState>();

        public InMemoryPlayerRepository(IPlayerPersistenceFactory playerPersistenceFactory)
        {
            _playerPersistenceFactory = playerPersistenceFactory;

            _playerIdToPlayer.Add(new PlayerId("ivan-id"), new PlayerState
            {
                PlayerId = new PlayerId("ivan-id"),
                LocationId = new LocationId("village"),
                RoadMovementState = null
            });

            _playerIdToPlayer.Add(new PlayerId("john-id"), new PlayerState
            {
                PlayerId = new PlayerId("john-id"),
                LocationId = new LocationId("village"),
                RoadMovementState = null
            });
        }

        public Player? Find(PlayerId playerId)
        {
            if (!_playerIdToPlayer.ContainsKey(playerId))
                return null;

            var state = _playerIdToPlayer[playerId];

            return _playerPersistenceFactory.FromState(state);
        }

        public void Save(Player player)
        {
            var state = player.GetState();

            if (_playerIdToPlayer.ContainsKey(state.PlayerId))
            {
                _playerIdToPlayer[state.PlayerId] = state;
                return;
            }

            _playerIdToPlayer.Add(state.PlayerId, state);
        }
    }
}
