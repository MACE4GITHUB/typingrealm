using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Authentication
{
    public interface IProfileService
    {
        ValueTask<bool> CharacterBelongsToCurrentProfileAsync(string characterId, CancellationToken cancellationToken);
    }
}
