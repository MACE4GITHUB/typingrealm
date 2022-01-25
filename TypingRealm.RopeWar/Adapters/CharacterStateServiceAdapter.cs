using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Profiles.Activities;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.RopeWar.Adapters;

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

        return currentActivity != null
            && currentActivity.ActivityId == contestId
            && currentActivity.Type == ActivityType.RopeWar;
    }
}
