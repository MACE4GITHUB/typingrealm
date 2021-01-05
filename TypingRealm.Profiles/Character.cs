using System;

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

        // TODO: Move from profiles to characters api.
        public string? ActivityId { get; private set; }

        public void Archive()
        {
            IsArchived = true;
        }

        public void EnterActivity(string activityId)
        {
            ActivityId = activityId;
        }

        public void LeaveActivity()
        {
            if (ActivityId == null)
                throw new InvalidOperationException("Not participating in activity.");

            ActivityId = null;
        }
    }
}
