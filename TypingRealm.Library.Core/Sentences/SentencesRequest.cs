using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Library.Sentences;

/// <summary>
/// Request to get Sentences from the Library.
/// </summary>
public sealed class SentencesRequest
{
    private const int MaxCountLimit = 10000;

    /// <summary>
    /// Type of request, whether you want random sentences or based on some criteria.
    /// </summary>
    public SentencesRequestType Type { get; init; }

    /// <summary>
    /// Maximum count of sentences. You might get less sentences than this count
    /// if they were not found based on your criteria, but you won't get more
    /// than this number.
    /// </summary>
    public int MaxCount { get; init; }

    /// <summary>
    /// Indicates how many sentences will be returned in a row, from some book.
    /// For example, if you want 10 random sentences but specify 5
    /// <see cref="ConsecutiveCount"/>, then you'll get 5 sentences from one book,
    /// and another 5 sentences from another book, but within those 5 sentences
    /// they'll be taken consecutively from the book (like a paragraph of text).
    /// </summary>
    public int ConsecutiveCount { get; init; } = 1;

    /// <summary>
    /// Specifies text parts that you need your text to contain. This should
    /// only be used with <see cref="SentencesRequestType"/> that contains words
    /// or key pairs.
    /// </summary>
    public IEnumerable<string> Contains { get; init; } = Enumerable.Empty<string>();

    public static SentencesRequest Random(int count) => Random(count, 1);
    public static SentencesRequest Random(int count, int consecutiveCount) => new SentencesRequest
    {
        Type = SentencesRequestType.Random,
        MaxCount = count,
        ConsecutiveCount = consecutiveCount
    };

    public static SentencesRequest ContainingKeyPairs(IEnumerable<string> keyPairs, int count) => new SentencesRequest
    {
        Type = SentencesRequestType.ContainingKeyPairs,
        MaxCount = count,
        Contains = keyPairs
    };

    public static SentencesRequest ContainingWords(IEnumerable<string> words, int count) => new SentencesRequest
    {
        Type = SentencesRequestType.ContainingWords,
        MaxCount = count,
        Contains = words
    };

    public bool IsValid()
    {
        return !GetErrorsIfInvalid().Any();
    }

    public IEnumerable<string> GetErrorsIfInvalid()
    {
        if (Type == SentencesRequestType.Unspecified)
            yield return $"{nameof(Type)} should be specified.";

        if (MaxCount <= 0)
            yield return $"{nameof(MaxCount)} should have positive value.";

        if (MaxCount > MaxCountLimit)
            yield return $"{nameof(MaxCount)} cannot exceed {MaxCountLimit} value.";

        if (ConsecutiveCount <= 0)
            yield return $"{nameof(ConsecutiveCount)} should have positive value.";

        if (ConsecutiveCount > MaxCount)
            yield return $"{nameof(ConsecutiveCount)} should be less or equal to {nameof(MaxCount)}";

        if (Type == SentencesRequestType.Random && Contains != null && Contains.Any())
            yield return $"{nameof(Contains)} should be empty when {nameof(WordsRequestType.Random)} type is used.";

        if (Type != SentencesRequestType.Random && (Contains == null || !Contains.Any()))
            yield return $"{nameof(Contains)} should not be empty when {nameof(Type)} is used that should contain something.";
    }
}
