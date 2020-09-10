namespace TypingRealm.Profiles.Api.Resources
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
