using System.Collections.Generic;
using TypingRealm.Profiles.Activities;

namespace TypingRealm.Profiles.Api.Resources;

public sealed record ActivityResource(
    string ActivityId,
    ActivityType Type,
    IEnumerable<string> CharacterIds);
