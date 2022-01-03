using System.Collections.Generic;
using TypingRealm.Texts;

namespace TypingRealm.Typing
{
    /// <param name="Length">If it's 0 - generates text of default length.</param>
    public sealed record TextGenerationConfigurationDto(
        int Length,
        IEnumerable<string> ShouldContain,
        TextStructure TextType,
        string Language);
}
