using System.Collections.Generic;
using TypingRealm.Messaging;

namespace TypingRealm.Domain.Messages
{
    [Message]
    public sealed class MovementUpdate
    {
#pragma warning disable CS8618
        public MovementUpdate() { }
#pragma warning restore CS8618
        public MovementUpdate(string roadId, PlayerPosition playerPosition, IEnumerable<PlayerPosition> playerPositions)
        {
            RoadId = roadId;
            PlayerPosition = playerPosition;
            PlayerPositions = playerPositions;
        }

        public string RoadId { get; set; }
        public PlayerPosition PlayerPosition { get; set; }
        public IEnumerable<PlayerPosition> PlayerPositions { get; set; }
    }

    [Message]
    public sealed class PlayerPosition
    {
#pragma warning disable CS8618
        public PlayerPosition() { }
#pragma warning restore CS8618
        public PlayerPosition(string playerId, int progress, bool isMovingForward)
        {
            PlayerId = playerId;
            Progress = progress;
            IsMovingForward = isMovingForward;
        }

        public string PlayerId { get; set; }
        public int Progress { get; set; }
        public bool IsMovingForward { get; set; }
    }
}
