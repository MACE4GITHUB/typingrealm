using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.World.Layers;

namespace TypingRealm.World.Activities.RopeWar
{
    public sealed class JoinRopeWarContestHandler : LayerHandler<JoinRopeWarContest>
    {
        private readonly ILocationStore _locationStore;

        public JoinRopeWarContestHandler(
            ICharacterActivityStore characterActivityStore,
            ILocationStore locationStore)
            : base(characterActivityStore, Layer.World)
        {
            _locationStore = locationStore;
        }

        protected override ValueTask HandleLayeredMessageAsync(ConnectedClient sender, JoinRopeWarContest message, CancellationToken cancellationToken)
        {
            var location = _locationStore.FindLocationForCharacter(sender.ClientId);
            if (location == null)
                throw new InvalidOperationException("Location does not exist.");

            if (location.LocationId != sender.Group)
                throw new InvalidOperationException("Synchronization mismatch.");

            var ropeWar = location.RopeWarActivities.Find(rw => rw.ActivityId == message.RopeWarId);
            if (ropeWar == null)
                throw new InvalidOperationException("RopeWar with this identity is not proposed yet.");

            if (ropeWar.LeftSideParticipants.Contains(sender.ClientId)
                || ropeWar.RightSideParticipants.Contains(sender.ClientId))
                throw new InvalidOperationException("You are already joined to this contest.");

            ropeWar.Join(sender.ClientId, message.Side);

            _locationStore.Save(location);
            return default;
        }
    }
}
