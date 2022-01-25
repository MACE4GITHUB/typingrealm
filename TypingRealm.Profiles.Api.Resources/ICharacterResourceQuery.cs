using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Profiles.Api.Resources;

public interface ICharacterResourceQuery
{
    ValueTask<IEnumerable<CharacterResource>> FindAllByProfileIdAsync(string profileId);
    ValueTask<CharacterResource?> FindByCharacterIdAsync(string characterId);
}
