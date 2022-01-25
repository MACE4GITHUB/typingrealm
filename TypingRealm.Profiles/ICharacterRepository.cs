using System.Threading.Tasks;

namespace TypingRealm.Profiles;

public interface ICharacterRepository
{
    ValueTask<Character?> FindAsync(CharacterId characterId);
    ValueTask SaveAsync(Character character);

    ValueTask<CharacterId> NextIdAsync();
}
