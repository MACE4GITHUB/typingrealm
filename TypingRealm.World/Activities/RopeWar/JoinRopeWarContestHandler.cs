using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.World.Layers;

namespace TypingRealm.World.Activities.RopeWar;

public sealed class JoinRopeWarContestHandler : LayerHandler<JoinRopeWarContest>
{
    private readonly ILocationRepository _locationStore;

    public JoinRopeWarContestHandler(
        ICharacterActivityStore characterActivityStore,
        ILocationRepository locationStore)
        : base(characterActivityStore, Layer.World)
    {
        _locationStore = locationStore;
    }

    protected override ValueTask HandleMessageAsync(ConnectedClient sender, JoinRopeWarContest message, CancellationToken cancellationToken)
    {
        var characterId = sender.ClientId;
        var location = _locationStore.FindLocationForClient(sender);

        location.JoinRopeWarContest(characterId, message.RopeWarId, message.Side);

        _locationStore.Save(location);
        return default;
    }
}
