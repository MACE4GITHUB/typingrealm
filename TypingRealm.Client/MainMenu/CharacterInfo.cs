using TypingRealm.Profiles.Api.Resources;

namespace TypingRealm.Client.MainMenu
{
    public sealed record CharacterInfo(string CharacterId, string Name)
    {
        public static CharacterInfo FromCharacterResource(CharacterResource characterResource)
        {
            return new CharacterInfo(characterResource.CharacterId, characterResource.Name);
        }
    }
}
