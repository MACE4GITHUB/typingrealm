using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Library.Sentences;

public sealed class WordsRequest
{
    public WordsRequestType Type { get; set; }
    public int MaxCount { get; set; }
    public int ConsecutiveCount { get; set; } = 1;
    public IEnumerable<string> Contains { get; set; } = Enumerable.Empty<string>();
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
        if (Contains == null)
            return false;

        if (Type == WordsRequestType.Random)
        {
            if (Contains.Any())
                return false;

            return IsValidCount();
        }

        if (Type == WordsRequestType.ContainingKeyPairs)
        {
            if (!Contains.Any())
                return false;

            return IsValidCount();
        }

        return false;
    }

    private bool IsValidCount()
    {
        if (MaxCount <= 0)
            return false;

        if (ConsecutiveCount <= 0)
            return false;

        return true;
    }
}
