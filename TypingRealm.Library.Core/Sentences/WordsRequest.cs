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

    public bool IsValid()
    {
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
