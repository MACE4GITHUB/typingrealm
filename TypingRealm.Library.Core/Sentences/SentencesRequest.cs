﻿using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Library.Sentences;

public sealed class SentencesRequest
{
    public SentencesRequestType Type { get; set; }
    public int MaxCount { get; set; }
    public int ConsecutiveCount { get; set; } = 1;
    public IEnumerable<string> Contains { get; set; } = Enumerable.Empty<string>();

    public static SentencesRequest Random(int count) => Random(count, 1);
    public static SentencesRequest Random(int count, int consecutiveCount) => new SentencesRequest
    {
        Type = SentencesRequestType.Random,
        MaxCount = count,
        ConsecutiveCount = consecutiveCount
    };

    public bool IsValid()
    {
        if (Type == SentencesRequestType.Random)
        {
            if (Contains.Any())
                return false;

            return IsValidCount();
        }

        if (Type == SentencesRequestType.ContainingWords)
        {
            if (!Contains.Any())
                return false;

            if (ConsecutiveCount != 1)
                return false;

            return IsValidCount();
        }

        if (Type == SentencesRequestType.ContainingKeyPairs)
        {
            if (!Contains.Any())
                return false;

            if (ConsecutiveCount != 1)
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