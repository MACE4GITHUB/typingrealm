using System.Collections.Generic;

namespace TypingRealm.Texts
{
    public enum TextGenerationType
    {
        Sentences = 1,
        Words = 2
    }

    // Here will be some additional properties like remove punctuation or only lowercase.
    public sealed record TextGenerationConfiguration(
        string Language,
        int Length,
        TextGenerationType TextType,
        IEnumerable<string> ShouldContain);
}
