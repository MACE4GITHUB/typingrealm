using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Library.Sentences;

/// <summary>
/// Request to get Words from the Library.
/// </summary>
public sealed class WordsRequest
{
    private const int MaxCountLimit = 10000;

    /// <summary>
    /// Type of request, whether you want random words or based on some criteria.
    /// </summary>
    public WordsRequestType Type { get; init; }

    /// <summary>
    /// Maximum count of words. You might get less words than this count if they
    /// were not found based on your criteria, but you won't get more than this
    /// number.
    /// </summary>
    public int MaxCount { get; init; }

    /// <summary>
    /// Indicates how many words will be returned in a row, from some book. If
    /// it's more than 1, then some of the words will follow each other just
    /// like in a regular sentence.
    /// </summary>
    public int ConsecutiveCount { get; set; } = 1;

    /// <summary>
    /// Specifies text parts that you need your words to contain. This works
    /// only with <see cref="WordsRequestType.ContainingKeyPairs" />.
    /// </summary>
    public IEnumerable<string> Contains { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Indicates whether words should be stripped from any punctuation and made
    /// lowercase, to get raw alphabet symbols. Can be useful for letter
    /// practice or for parts of menus and other in-game interactions.
    /// </summary>
    public bool RawWords { get; set; }

    public static WordsRequest Random(int count) => Random(count, 1);
    public static WordsRequest Random(int count, int consecutiveCount) => new WordsRequest
    {
        Type = WordsRequestType.Random,
        MaxCount = count,
        ConsecutiveCount = consecutiveCount
    };

    public static WordsRequest ContainingKeyPairs(IEnumerable<string> keyPairs, int count) => new WordsRequest
    {
        Type = WordsRequestType.ContainingKeyPairs,
        MaxCount = count,
        Contains = keyPairs
    };

    public bool IsValid()
    {
        return !GetErrorsIfInvalid().Any();
    }

    public IEnumerable<string> GetErrorsIfInvalid()
    {
        if (Type == WordsRequestType.Unspecified)
            yield return $"{nameof(Type)} should be specified.";

        if (MaxCount <= 0)
            yield return $"{nameof(MaxCount)} should have positive value.";

        if (MaxCount > MaxCountLimit)
            yield return $"{nameof(MaxCount)} cannot exceed {MaxCountLimit} value.";

        if (ConsecutiveCount <= 0)
            yield return $"{nameof(ConsecutiveCount)} should have positive value.";

        if (ConsecutiveCount > MaxCount)
            yield return $"{nameof(ConsecutiveCount)} should be less or equal to {nameof(MaxCount)}";

        if (Type == WordsRequestType.Random && Contains != null && Contains.Any())
            yield return $"{nameof(Contains)} should be empty when {nameof(WordsRequestType.Random)} type is used.";

        if (Type != WordsRequestType.Random && (Contains == null || !Contains.Any()))
            yield return $"{nameof(Contains)} should not be empty when {nameof(Type)} is used that should contain something.";
    }
}
