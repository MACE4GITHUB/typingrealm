using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Profiles.Api.Resources;
using TypingRealm.Profiles.Api.Resources.Data;

namespace TypingRealm.Profiles.Api.Client
{
    public interface ICharactersClient
    {
        ValueTask<bool> BelongsToCurrentProfileAsync(string characterId, CancellationToken cancellationToken);
        ValueTask<IEnumerable<CharacterResource>> GetAllByProfileIdAsync(CancellationToken cancellationToken);
        ValueTask CreateAsync(CreateCharacterDto dto, CancellationToken cancellationToken);
    }
}
