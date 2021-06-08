using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.World.Activities.RopeWar
{
    public sealed class ProposeRopeWarContestHandler : IMessageHandler<ProposeRopeWarContest>
    {
        private readonly ILocationStore _locationStore;
        private readonly ICharactersClient _charactersClient;

        public ProposeRopeWarContestHandler(ILocationStore locationStore, ICharactersClient charactersClient)
        {
            _locationStore = locationStore;
            _charactersClient = charactersClient;
        }

        public async ValueTask HandleAsync(ConnectedClient sender, ProposeRopeWarContest message, CancellationToken cancellationToken)
        {
            var characterId = sender.ClientId;

            // TODO: Subtract "bet" from character money (or put those money on HOLD).
            // Return them if anything bad happened and contest did not actually work / was canceled / he won eventually.

            var location = _locationStore.FindLocationForCharacter(sender.ClientId);
            if (location == null)
                throw new InvalidOperationException("Location does not exist.");

            if (location.LocationId != sender.Group)
                throw new InvalidOperationException("Synchronization mismatch.");

            if (!location.CanProposeRopeWar)
                throw new InvalidOperationException("Cannot propose ropewar here.");

            var activityId = Guid.NewGuid().ToString();

            // TODO: Store Activity in World domain first and foremost, it's not a Character API concern.
            // Just update Characters API just for a central data store that can be accessed by other domains to check
            // whether it can serve this client.
            // Maybe even create a separate api apart from profile - high-performant ACL-like Character Data Cache.
            // TODO: Check if character is already participating in any other activity - he cannot propose or join any other activity.
            await _charactersClient.EnterActivityAsync(characterId, activityId, cancellationToken)
                .ConfigureAwait(false);

            var activity = new RopeWarActivity(activityId, message.Name, characterId, message.Bet);
            activity.Join(characterId, message.Side);
            location.RopeWarActivities.Add(activity);

            _locationStore.Save(location);
        }
    }
}
