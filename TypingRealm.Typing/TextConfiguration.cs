﻿using System.Collections.Generic;

namespace TypingRealm.Typing
{
    public enum TextType
    {
        Text = 1,
        Words = 2
    }

    /// <param name="Length">If it's 0 - generates text of default length.</param>
    public sealed record TextConfiguration(
        int Length,
        IEnumerable<string> ShouldContain,
        TextType TextType);
}
