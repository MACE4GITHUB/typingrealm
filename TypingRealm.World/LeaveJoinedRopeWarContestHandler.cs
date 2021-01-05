using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace TypingRealm.World
{
    public sealed class LeaveJoinedRopeWarContestHandler : IMessageHandler<LeaveJoinedRopeWarContest>
    {
        private readonly ILocationStore _locationStore;

        public LeaveJoinedRopeWarContestHandler(ILocationStore locationStore)
        {
            _locationStore = locationStore;
        }

        public ValueTask HandleAsync(ConnectedClient sender, LeaveJoinedRopeWarContest message, CancellationToken cancellationToken)
        {
            var location = _locationStore.FindLocationForClient(sender);
            var characterId = sender.ClientId;

            var ropeWar = location.RopeWars.FirstOrDefault(rw => rw.LeftSideParticipants.Contains(characterId) || rw.RightSideParticipants.Contains(characterId));
            if (ropeWar == null)
                throw new InvalidOperationException("Not in rope war in current location.");

            // TODO: Encapsulate such actions.
            ropeWar.LeftSideParticipants.Remove(characterId);
            ropeWar.RightSideParticipants.Remove(characterId);
            ropeWar.Votes.Clear();
            _locationStore.Save(location);
            return default;
        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
