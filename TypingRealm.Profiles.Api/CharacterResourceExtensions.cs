using TypingRealm.Profiles.Api.Resources;

namespace TypingRealm.Profiles.Api
{
    public static class CharacterResourceExtensions
    {
        public static CharacterResource ToCharacterResource(this Character character)
        {
            return new CharacterResource(
                character.ProfileId,
                character.CharacterId,
                character.Name);
        }
    }
}
