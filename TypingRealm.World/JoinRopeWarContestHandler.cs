using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace TypingRealm.World
{
    public sealed class JoinRopeWarContestHandler : IMessageHandler<JoinRopeWarContest>
    {
        private readonly ILocationStore _locationStore;

        public JoinRopeWarContestHandler(ILocationStore locationStore)
        {
            _locationStore = locationStore;
        }

        public ValueTask HandleAsync(ConnectedClient sender, JoinRopeWarContest message, CancellationToken cancellationToken)
        {
            var location = _locationStore.FindLocationForCharacter(sender.ClientId);
            if (location == null)
                throw new InvalidOperationException("Location does not exist.");

            if (location.LocationId != sender.Group)
                throw new InvalidOperationException("Synchronization mismatch.");

            var ropeWar = location.RopeWars.Find(rw => rw.ActivityId == message.RopeWarId);
            if (ropeWar == null)
                throw new InvalidOperationException("RopeWar with this identity is not proposed yet.");

            if (ropeWar.LeftSideParticipants.Contains(sender.ClientId)
                || ropeWar.RightSideParticipants.Contains(sender.ClientId))
                throw new InvalidOperationException("You are already joined to this contest.");

            switch (message.Side)
            {
                case RopeWarSide.Left:
                    ropeWar.LeftSideParticipants.Add(sender.ClientId);
                    break;
                case RopeWarSide.Right:
                    ropeWar.RightSideParticipants.Add(sender.ClientId);
                    break;
                default:
                    throw new InvalidOperationException("Unknown rope war side.");
            }

            _locationStore.Save(location);
            return default;
        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
