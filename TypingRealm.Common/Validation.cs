using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm;

public static class Validation
{
    public static void ValidateLength(string value, int from, int to)
    {
        if (value.Length < from || value.Length > to)
            throw new ArgumentException($"Value length should be between {from} and {to}.");
    }

    public static void ValidateIn(string value, IEnumerable<string> validValues)
    {
        if (!validValues.Any(x => value == x))
            throw new ArgumentException($"Value is not in the range of valid values.");
    }
}
