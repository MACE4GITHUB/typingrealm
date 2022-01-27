using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm;

public static class Validation
{
    /// <summary>
    /// Throws <see cref="ArgumentException"/> when value has length less than
    /// specified from characters, or more than specified to characters.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public static void ValidateLength(string value, int from, int to)
    {
        if (value.Length < from || value.Length > to)
            throw new ArgumentException($"Value length should be between {from} and {to}.");
    }

    /// <summary>
    /// Throws <see cref="ArgumentException"/> when supplied valid values
    /// collection does not contain the value.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public static void ValidateIn(string value, IEnumerable<string> validValues)
    {
        if (!validValues.Any(x => value == x))
            throw new ArgumentException($"Value is not in the range of valid values.");
    }
}
