using System.Collections.Generic;

namespace TypingRealm.Profiles.Activities;

public interface IActivityRepository
{
    bool ValidateCharactersDoNotHaveActiveActivity(IEnumerable<string> characterIds);
    Activity? Find(string activityId);
    Activity? FindActiveActivityForCharacter(string characterId);

    void Save(Activity activity);
}
