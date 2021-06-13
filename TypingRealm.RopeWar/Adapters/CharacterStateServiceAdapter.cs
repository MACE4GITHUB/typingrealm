using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.RopeWar.Adapters
{
    // TODO: Consider getting rid of this adapter now that we have ICharactersClient.
    public sealed class CharacterStateServiceAdapter : ICharacterStateService
    {
        private readonly IActivitiesClient _activitiesClient;

        public CharacterStateServiceAdapter(IActivitiesClient activitiesClient)
        {
            _activitiesClient = activitiesClient;
        }

        public async ValueTask<bool> CanJoinRopeWarContestAsync(string characterId, string contestId, CancellationToken cancellationToken)
        {
            var currentActivity = await _activitiesClient.GetCurrentActivityAsync(characterId, cancellationToken)
                .ConfigureAwait(false);

            // TODO: Also check that given activity is exactly of RopeWar type!!!
            return currentActivity != null && currentActivity == contestId;
        }
    }
}
