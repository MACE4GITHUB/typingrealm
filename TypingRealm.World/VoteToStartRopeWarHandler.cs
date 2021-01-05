using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace TypingRealm.World
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
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
