using System.Collections.Generic;

namespace TypingRealm.Profiles
{
    public interface IActivityRepository
    {
        bool ValidateCharactersDoNotHaveActivity(IEnumerable<string> characterIds);
        Activity? Find(string activityId);
        Activity? FindForCharacter(string characterId);

        void Save(Activity activity);
    }
}
