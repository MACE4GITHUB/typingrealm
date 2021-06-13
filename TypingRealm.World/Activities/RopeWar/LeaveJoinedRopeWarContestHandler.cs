using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.World.Layers;

namespace TypingRealm.World.Activities.RopeWar
{
    public sealed class LeaveJoinedRopeWarContestHandler : LayerHandler<LeaveJoinedRopeWarContest>
    {
        private readonly ILocationRepository _locationStore;

        public LeaveJoinedRopeWarContestHandler(
            ICharacterActivityStore characterActivityStore, ILocationRepository locationStore)
            : base(characterActivityStore, Layer.RopeWar)
        {
            _locationStore = locationStore;
        }

        protected override ValueTask HandleMessageAsync(ConnectedClient sender, LeaveJoinedRopeWarContest message, CancellationToken cancellationToken)
        {
            var location = _locationStore.FindLocationForClient(sender);
            var characterId = sender.ClientId;

            location.LeaveRopeWarContest(characterId);

            _locationStore.Save(location);

            return default;
        }
    }
}
