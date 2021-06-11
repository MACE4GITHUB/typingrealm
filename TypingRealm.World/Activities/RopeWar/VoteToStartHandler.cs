using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.World.Layers;

namespace TypingRealm.World.Activities.RopeWar
{
    public sealed class VoteToStartHandler : LayerHandler<VoteToStart>
    {
        private readonly ILocationStore _locationStore;

        public VoteToStartHandler(
            ICharacterActivityStore characterActivityStore, ILocationStore locationStore)
            : base(characterActivityStore, Layer.RopeWar)
        {
            _locationStore = locationStore;
        }

        protected override ValueTask HandleMessageAsync(ConnectedClient sender, VoteToStart message, CancellationToken cancellationToken)
        {
            var location = _locationStore.FindLocationForClient(sender);
            var characterId = sender.ClientId;

            var ropeWar = location.RopeWarActivities.FirstOrDefault(rw => rw.LeftSideParticipants.Contains(characterId) || rw.RightSideParticipants.Contains(characterId));
            if (ropeWar == null)
                throw new InvalidOperationException("Not in rope war in current location.");

            ropeWar.VoteToStart(characterId);

            _locationStore.Save(location);

            if (ropeWar.HasStarted)
                return CharacterActivityStore.EnterActivityAsync(characterId, ropeWar.ActivityId, cancellationToken);

            return default;
        }
    }
}
