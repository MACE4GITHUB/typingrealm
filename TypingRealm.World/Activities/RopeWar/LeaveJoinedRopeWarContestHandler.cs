using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;

namespace TypingRealm.World.Activities.RopeWar
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

            var ropeWar = location.RopeWarActivities.FirstOrDefault(rw => rw.LeftSideParticipants.Contains(characterId) || rw.RightSideParticipants.Contains(characterId));
            if (ropeWar == null)
                throw new InvalidOperationException("Not in rope war in current location.");

            ropeWar.Leave(characterId);

            _locationStore.Save(location);
            return default;
        }
    }
}
