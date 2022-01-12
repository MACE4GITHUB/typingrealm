namespace TypingRealm.Library.Sentences;

public sealed record KeyPair(
    int IndexInWord,
    string Value,
    int CountInWord,
    int CountInSentence);
