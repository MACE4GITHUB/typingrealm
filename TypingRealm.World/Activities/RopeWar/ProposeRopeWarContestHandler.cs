using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.World.Layers;

namespace TypingRealm.World.Activities.RopeWar
{
    public sealed class ProposeRopeWarContestHandler : LayerHandler<ProposeRopeWarContest>
    {
        private readonly ILocationStore _locationStore;

        public ProposeRopeWarContestHandler(
            ICharacterActivityStore characterActivityStore,
            ILocationStore locationStore)
            : base(characterActivityStore, Layer.World)
        {
            _locationStore = locationStore;
        }

        protected override ValueTask HandleMessageAsync(ConnectedClient sender, ProposeRopeWarContest message, CancellationToken cancellationToken)
        {
            var characterId = sender.ClientId;

            // TODO: Subtract "bet" from character money (or put those money on HOLD).
            // Return them if anything bad happened and contest did not actually work / was canceled / he won eventually.

            var location = _locationStore.FindLocationForClient(sender);

            // TODO: It is being checked inside Location. Remove this code.
            /*if (!location.CanProposeRopeWar)
                throw new InvalidOperationException("Cannot propose ropewar here.");*/

            // TODO: Store Activity in World domain first and foremost, it's not a Character API concern.
            // Just update Characters API just for a central data store that can be accessed by other domains to check
            // whether it can serve this client.
            // Maybe even create a separate api apart from profile - high-performant ACL-like Character Data Cache.
            // TODO: Check if character is already participating in any other activity - he cannot propose or join any other activity.

            location.ProposeRopeWarContest(characterId, message.Name, message.Bet, message.Side);

            _locationStore.Save(location);
            return default;
        }
    }
}
