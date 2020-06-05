using System;
using System.Linq;
using TypingRealm.Domain.Messages;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Domain
{
    public sealed class UpdateFactory : IUpdateFactory
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IConnectedClientStore _connectedClients;

        public UpdateFactory(
            IPlayerRepository playerRepository,
            IConnectedClientStore connectedClients)
        {
            _playerRepository = playerRepository;
            _connectedClients = connectedClients;
        }

        public object GetUpdateFor(string clientId)
        {
            var playerId = new PlayerId(clientId);
            var player = _playerRepository.Find(playerId);
            if (player == null)
                throw new InvalidOperationException("Player is not found.");

            if (player.CombatEnemyId != null)
                return new CombatUpdate(player.CombatEnemyId);

            var visiblePlayers = _connectedClients.FindInGroups(player.GetUniquePlayerPosition())
                .Select(client => client.ClientId);

            return new Update(player.LocationId, visiblePlayers);
        }
    }
}
