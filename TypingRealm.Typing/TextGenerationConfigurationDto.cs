using System.Collections.Generic;
using TypingRealm.Texts.Api.Client;

namespace TypingRealm.Typing
{
    /// <param name="Length">If it's 0 - generates text of default length.</param>
    public sealed record TextGenerationConfigurationDto(
        int Length,
        IEnumerable<string> ShouldContain,
        TextGenerationType TextType,
        string Language);
}
