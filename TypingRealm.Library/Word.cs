using System.Collections.Generic;
using TypingRealm.Texts;

namespace TypingRealm.Library;

// Immutable entity of Sentence aggregate root (with all public fields, record).
// Count is how many times this word is present in the sentence.
public sealed record Word(
    SentenceId SentenceId,
    //WordId WordId,
    int IndexInSentence,
    string Value,
    int CountInSentence,
    IEnumerable<KeyPair> KeyPairs)
{
    public string RawWord => TextHelpers.GetRawWord(Value);
}
