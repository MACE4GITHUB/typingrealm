using System;
using System.Linq;
using System.Text.RegularExpressions;
using TypingRealm.TextProcessing;

namespace TypingRealm.Library.Importing;

public sealed class SentenceValidator
{
    private readonly Regex _multipleUppercaseCharactersRegex = new(@".*\p{Lu}\p{Lu}.*", RegexOptions.Compiled);
    private const int MinSentenceLengthCharacters = 8;

    public bool IsValidSentence(string sentence)
    {
        ArgumentNullException.ThrowIfNull(sentence);

        if (sentence.Trim().Length < MinSentenceLengthCharacters)
            return false;

        if (_multipleUppercaseCharactersRegex.IsMatch(sentence))
            return false;

        if (sentence.All(character => TextConstants.AllNonLetterCharacters.Contains(character) || character == TextConstants.SpaceCharacter))
            return false;

        return true;
    }
}
