using System;
using System.Collections.Generic;

namespace TypingRealm.Domain
{
    public sealed class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly Dictionary<string, Player> _clientIdToPlayer
            = new Dictionary<string, Player>();
        private readonly Dictionary<string, Player> _playerIdToPlayer
            = new Dictionary<string, Player>();

        public Player GetByClientId(string clientId)
        {
            if (!_clientIdToPlayer.ContainsKey(clientId))
                throw new InvalidOperationException($"Player with client id {clientId} does not exist.");

            return _clientIdToPlayer[clientId];
        }

        public Player GetByPlayerId(string playerId)
        {
            if (!_playerIdToPlayer.ContainsKey(playerId))
                throw new InvalidOperationException($"Player with player id {playerId} does not exist.");

            return _playerIdToPlayer[playerId];
        }

        public IEnumerable<Player> GetPlayersVisibleTo(string playerId)
        {
            var player = _playerIdToPlayer[playerId];
            var uniqueLocation = player.GetUniqueLocation();

            foreach (var visiblePlayer in _playerIdToPlayer.Values)
            {
                if (visiblePlayer.GetUniqueLocation() == uniqueLocation)
                    yield return visiblePlayer;
            }
        }

        public void Save(string clientId, Player player)
        {
            if (_playerIdToPlayer.ContainsKey(player.Id))
            {
                _playerIdToPlayer[player.Id] = player;

                if (!_clientIdToPlayer.ContainsKey(clientId))
                    _clientIdToPlayer.Add(clientId, player);

                _clientIdToPlayer[clientId] = player;
                return;
            }

            _playerIdToPlayer.Add(player.Id, player);

            if (!_clientIdToPlayer.ContainsKey(clientId))
                _clientIdToPlayer.Add(clientId, player);

            _clientIdToPlayer[clientId] = player;
        }
    }
}
