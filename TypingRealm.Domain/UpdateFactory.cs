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
                throw new InvalidOperationException($"Player {playerId} is not found.");

            var state = player.GetState();

            if (state.RoadMovementState != null)
            {
                var playerPositions = _connectedClients.FindInGroups(player.MessagingGroup)
                    .Select(client =>
                    {
                        var playerId = new PlayerId(client.ClientId);
                        var player = _playerRepository.Find(playerId);
                        if (player == null)
                            throw new InvalidOperationException($"Player {playerId} is not found.");

                        return player;
                    })
                    .Select(p => ToPlayerPosition(p, state.RoadMovementState.MovementDirection));

                return new MovementUpdate(
                    state.RoadMovementState.RoadId,
                    playerPositions.Single(p => p.PlayerId == state.PlayerId),
                    playerPositions.ToList());
            }

            var visiblePlayerIds = _connectedClients.FindInGroups(player.MessagingGroup)
                .Select(client => client.ClientId)
                .ToList();

            return new Update(player.MessagingGroup, visiblePlayerIds);
        }

        private PlayerPosition ToPlayerPosition(Player player, MovementDirection currentDirection)
        {
            var movement = player.RoadMovementComponent;
            if (movement == null)
                throw new InvalidOperationException("Player is not moving.");

            var distance = movement.Distance;
            var progress = movement.Progress;

            if (movement.Direction != currentDirection)
                progress = movement.Road.GetDistanceFor(currentDirection) - new Distance((int)Math.Floor((double)progress.Value * movement.Road.GetDistanceFor(movement.Direction.Flip()).Value / distance.Value));

            return new PlayerPosition(player.GetState().PlayerId, progress, movement.Direction == MovementDirection.Forward);
        }
    }
}
