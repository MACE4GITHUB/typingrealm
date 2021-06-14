using System.Collections.Generic;

namespace TypingRealm.Profiles.Api.Resources
{
    public sealed record ActivityResource(
        string ActivityId,
        ActivityType Type,
        IEnumerable<string> CharacterIds);
}
