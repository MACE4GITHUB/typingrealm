using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TypingRealm.Profiles.Api.Resources;

namespace TypingRealm.Profiles.Infrastructure;

public sealed class InMemoryCharacterResourceQuery : ICharacterResourceQuery
{
    private readonly InMemoryCharacterRepository _characterRepository;

    public InMemoryCharacterResourceQuery(ICharacterRepository characterRepository)
    {
        if (characterRepository is InMemoryCharacterRepository inMemoryRepository)
        {
            _characterRepository = inMemoryRepository;
            return;
        }

        throw new NotSupportedException($"Character repository is not {nameof(InMemoryCharacterRepository)}.");
    }

    public ValueTask<IEnumerable<CharacterResource>> FindAllByProfileIdAsync(string profileId)
    {
        return new ValueTask<IEnumerable<CharacterResource>>(_characterRepository._cache.Values
            .Where(character => !character.IsArchived)
            .Select(character => new CharacterResource(
                character.ProfileId,
                character.CharacterId,
                character.Name))
            .ToList());
    }

    public ValueTask<CharacterResource?> FindByCharacterIdAsync(string characterId)
    {
        _characterRepository._cache.TryGetValue(new CharacterId(characterId), out var character);

        var result = (character == null || character.IsArchived) ? null : new CharacterResource(
            character.ProfileId,
            character.CharacterId,
            character.Name);

        return new ValueTask<CharacterResource?>(result);
    }
}
