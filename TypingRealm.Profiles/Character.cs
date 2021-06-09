using System;
using System.Collections.Generic;

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

        public Stack<string> Activities { get; } = new Stack<string>();

        public CharacterId CharacterId { get; }
        public ProfileId ProfileId { get; }
        public CharacterName Name { get; set; }
        public bool IsArchived { get; private set; }

        // TODO: Move from profiles to characters api.
        public string? CurrentActivityId
        {
            get
            {
                if (Activities.TryPeek(out var activity))
                    return activity;

                return null;
            }
        }

        public void Archive()
        {
            IsArchived = true;
        }

        public void EnterActivity(string activityId)
        {
            Activities.Push(activityId);
        }

        public void LeaveActivity()
        {
            if (CurrentActivityId == null)
                throw new InvalidOperationException("Not participating in activity.");

            Activities.Pop();
        }
    }
}
