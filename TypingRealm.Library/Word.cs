using System.Collections.Generic;

namespace TypingRealm.Library;

// Immutable entity of Sentence aggregate root (with all public fields, record).
// Count is how many times this word is present in the sentence.
public sealed record Word(
    SentenceId SentenceId,
    //WordId WordId,
    int Index,
    string Value,
    int CountInSentence,
    IEnumerable<KeyPair> KeyPairs);
