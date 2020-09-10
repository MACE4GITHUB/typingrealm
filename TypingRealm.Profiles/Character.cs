namespace TypingRealm.Profiles
{
    public sealed class Character
    {
        public Character(CharacterId characterId, ProfileId profileId, CharacterName name)
        {
            CharacterId = characterId;
            ProfileId = profileId;
            Name = name;
        }

        public CharacterId CharacterId { get; }
        public ProfileId ProfileId { get; }
        public CharacterName Name { get; set; }
        public bool IsArchived { get; private set; }

        public void Archive()
        {
            IsArchived = true;
        }
    }
}
