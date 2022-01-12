using System.Collections.Generic;

namespace TypingRealm.Library.Sentences;

public sealed record Word(
    SentenceId SentenceId,
    int IndexInSentence,
    string Value,
    string RawValue,
    int CountInSentence,
    int RawCountInSentence,
    IEnumerable<KeyPair> KeyPairs);
