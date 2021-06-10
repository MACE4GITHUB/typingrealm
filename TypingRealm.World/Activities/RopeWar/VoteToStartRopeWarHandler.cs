using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;

namespace TypingRealm.World.Activities.RopeWar
{
    public sealed class VoteToStartRopeWarHandler : IMessageHandler<VoteToStartRopeWar>
    {
        private readonly ILocationStore _locationStore;

        public VoteToStartRopeWarHandler(ILocationStore locationStore)
        {
            _locationStore = locationStore;
        }

        public ValueTask HandleAsync(ConnectedClient sender, VoteToStartRopeWar message, CancellationToken cancellationToken)
        {
            var location = _locationStore.FindLocationForClient(sender);

            location.VoteToStartRopeWar(sender.ClientId);

            _locationStore.Save(location);
            return default;
        }
    }
}
