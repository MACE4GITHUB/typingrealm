using System.Collections.Generic;

namespace TypingRealm.Typing
{
    /// <param name="Length">If it's 0 - generates text of default length.</param>
    public sealed record TextConfiguration(
        int Length,
        IEnumerable<string> ShouldContain);
}
