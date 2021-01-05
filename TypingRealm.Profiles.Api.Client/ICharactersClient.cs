using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Profiles.Api.Client
{
    public interface ICharactersClient
    {
        ValueTask<bool> BelongsToCurrentProfileAsync(string characterId, CancellationToken cancellationToken);

        ValueTask<bool> CanJoinRopeWarContestAsync(string characterId, string contestId, CancellationToken cancellationToken);

        ValueTask EnterActivityAsync(string characterId, string activityId, CancellationToken cancellationToken);
    }
}
