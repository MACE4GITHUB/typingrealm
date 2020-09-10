namespace TypingRealm.Profiles.Api.Resources
{
    public sealed class CharacterResource
    {
        public CharacterResource(string profileId, string characterId, string name)
        {
            ProfileId = profileId;
            CharacterId = characterId;
            Name = name;
        }

        public string ProfileId { get; }
        public string CharacterId { get; }
        public string Name { get; }
    }
}
