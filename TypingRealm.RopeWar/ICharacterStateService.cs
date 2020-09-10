using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.RopeWar
{
    public interface ICharacterStateService
    {
        ValueTask<bool> CanJoinRopeWarContestAsync(string characterId, string contestId, CancellationToken cancellationToken);
    }
}
