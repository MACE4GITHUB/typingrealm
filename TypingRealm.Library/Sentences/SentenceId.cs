using System;

namespace TypingRealm.Library.Sentences;

public sealed class SentenceId : Identity
{
    public const int MaxLength = 50;

    public SentenceId(string value) : base(value)
    {
        Validation.ValidateLength(value, 1, MaxLength);
    }

    public static SentenceId New() => new(Guid.NewGuid().ToString());
}
