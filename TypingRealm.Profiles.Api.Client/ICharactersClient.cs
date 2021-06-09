using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Profiles.Api.Client
{
    public interface ICharactersClient
    {
        ValueTask<bool> BelongsToCurrentProfileAsync(string characterId, CancellationToken cancellationToken);

        ValueTask<bool> CanJoinActivityAsync(string characterId, string activityId, CancellationToken cancellationToken);
        ValueTask EnterActivityAsync(string characterId, string activityId, CancellationToken cancellationToken);
        ValueTask LeaveActivityAsync(string characterId, string activityId, CancellationToken cancellationToken);
        ValueTask<Stack<string>> GetActivitiesAsync(string characterId, CancellationToken cancellationToken);
    }
}
