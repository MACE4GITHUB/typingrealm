using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.World.Layers;

namespace TypingRealm.World.Activities.RopeWar
{
    public sealed class VoteToStartRopeWarHandler : LayerHandler<VoteToStartRopeWar>
    {
        private readonly ILocationRepository _locationStore;

        public VoteToStartRopeWarHandler(
            ICharacterActivityStore characterActivityStore,
            ILocationRepository locationStore)
            : base(characterActivityStore, Layer.RopeWar)
        {
            _locationStore = locationStore;
        }

        protected override ValueTask HandleMessageAsync(ConnectedClient sender, VoteToStartRopeWar message, CancellationToken cancellationToken)
        {
            var location = _locationStore.FindLocationForClient(sender);

            location.VoteToStartRopeWar(sender.ClientId);

            _locationStore.Save(location);
            return default;
        }
    }
}
