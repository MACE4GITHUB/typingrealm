using System;
using System.Collections.Generic;

namespace TypingRealm.Domain
{
    public sealed class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly Dictionary<string, Player> _clientIdToPlayer
            = new Dictionary<string, Player>();
        private readonly Dictionary<PlayerId, Player> _playerIdToPlayer
            = new Dictionary<PlayerId, Player>();

        public Player FindByClientId(string clientId)
        {
            if (!_clientIdToPlayer.ContainsKey(clientId))
                throw new InvalidOperationException($"Player with client id {clientId} does not exist.");

            return _clientIdToPlayer[clientId];
        }

        public Player FindByPlayerId(PlayerId playerId)
        {
            if (!_playerIdToPlayer.ContainsKey(playerId))
                throw new InvalidOperationException($"Player with player id {playerId} does not exist.");

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

        public void Save(string clientId, Player player)
        {
            if (_playerIdToPlayer.ContainsKey(player.PlayerId))
            {
                _playerIdToPlayer[player.PlayerId] = player;

                if (!_clientIdToPlayer.ContainsKey(clientId))
                    _clientIdToPlayer.Add(clientId, player);

                _clientIdToPlayer[clientId] = player;
                return;
            }

            _playerIdToPlayer.Add(player.PlayerId, player);

            if (!_clientIdToPlayer.ContainsKey(clientId))
                _clientIdToPlayer.Add(clientId, player);

            _clientIdToPlayer[clientId] = player;
        }
    }
}
