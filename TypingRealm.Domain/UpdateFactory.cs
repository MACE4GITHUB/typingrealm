using System;
using System.Linq;
using TypingRealm.Domain.Messages;
using TypingRealm.Domain.Movement;
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

            if (player.MovementComponent != null)
            {
                var playerPositions = _connectedClients.FindInGroups(player.MovementComponent!.Road.RoadId)
                    .Select(client => _playerRepository.Find(new PlayerId(client.ClientId)))
                    .Select(p => ToPlayerPosition(p!, player.MovementComponent.Direction));

                return new MovementUpdate(
                    player.MovementComponent.Road.RoadId,
                    playerPositions.Single(p => p.PlayerId == player.PlayerId),
                    playerPositions.ToList());
            }

            var visiblePlayerIds = _connectedClients.FindInGroups(player.LocationId)
                .Select(client => client.ClientId)
                .ToList();

            return new Update(player.LocationId, visiblePlayerIds);
        }

        private PlayerPosition ToPlayerPosition(Player player, RoadDirection currentDirection)
        {
            var movement = player.MovementComponent;
            if (movement == null)
                throw new InvalidOperationException("Player is not moving.");

            var distance = movement.Distance;
            var progress = movement.Progress;

            if (movement.Direction != currentDirection)
                progress = movement.Road.GetDistanceFor(currentDirection) - new Distance((int)Math.Floor((double)progress.Value * movement.Road.GetDistanceFor(movement.Direction.Flip()).Value / distance.Value));

            return new PlayerPosition(player.PlayerId, progress, movement.Direction == RoadDirection.Forward);
        }
    }
}
