using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypingRealm.Texts;

public interface ITextGenerator
{
    ValueTask<GeneratedText> GenerateTextAsync(TextGenerationConfiguration configuration);
}

public sealed class TextGenerator : ITextGenerator
{
    private const int MaxAllowedTextLength = 1000;

    private readonly TextRetrieverResolver _textRetrieverResolver;

    public TextGenerator(TextRetrieverResolver textRetrieverResolver)
    {
        _textRetrieverResolver = textRetrieverResolver;
    }

    public async ValueTask<GeneratedText> GenerateTextAsync(TextGenerationConfiguration configuration)
    {
        Validate(configuration);

        var requiredLength = configuration.Length;
        var shouldContain = configuration.IsLowerCase
            ? configuration.ShouldContain.Select(part => part.ToLowerInvariant())
            : configuration.ShouldContain;

        var textRetriever = _textRetrieverResolver(configuration.Language);

        var builder = new StringBuilder();

        var maxTries = 200; // Avoid endless loop because of missing shouldContain chunks.
        var tries = 0;

        while (builder.Length < Math.Min(requiredLength, MaxAllowedTextLength))
        {
            var text = await textRetriever.RetrieveTextAsync()
                .ConfigureAwait(false);

            var textPartsEnumerable = configuration.Structure switch
            {
                TextStructure.Text => TextHelpers.GetSentencesEnumerable(text),
                TextStructure.Words => TextHelpers.GetWordsEnumerable(text),
                _ => throw new InvalidOperationException("Unknown text structure.")
            };

            if (configuration.IsLowerCase)
                textPartsEnumerable = textPartsEnumerable.Select(part => part.ToLowerInvariant());

            if (configuration.StripPunctuation)
                textPartsEnumerable = textPartsEnumerable.Select(part =>
                {
                    foreach (var character in SupportedLanguages.PunctuationCharacters)
                    {
                        part = part.Replace(character.ToString(), "");
                    }

                    return part;
                });

            if (configuration.StripNumbers)
                textPartsEnumerable = textPartsEnumerable.Select(part =>
                {
                    foreach (var character in SupportedLanguages.NumberCharacters)
                    {
                        part = part.Replace(character.ToString(), "");
                    }

                    return part;
                });

            foreach (var textPart in textPartsEnumerable)
            {
                if (!IsAllowed(textPart, shouldContain) && tries < maxTries)
                {
                    tries++;
                    continue;
                }

                if (builder.Length == 0)
                {
                    builder.Append(textPart);
                }
                else
                {
                    builder.Append($" {textPart}");
                }

                if (builder.Length >= Math.Min(requiredLength, MaxAllowedTextLength))
                    break;
            }
        }

        return new GeneratedText(builder.ToString(), configuration);
    }

    private static void Validate(TextGenerationConfiguration configuration)
    {
        if (configuration.Length < 0 || configuration.Length > MaxAllowedTextLength)
            throw new ArgumentException($"Invalid configuration: length should be positive number below or equal to {MaxAllowedTextLength}.");

        if (!SupportedLanguages.Languages.Contains(configuration.Language))
            throw new ArgumentException($"Language {configuration.Language} is not supported.");
    }

    private static bool IsAllowed(string word, IEnumerable<string> shouldContain)
    {
        if (!shouldContain.Any())
            return true;

        foreach (var piece in shouldContain)
        {
            if (word.Contains(piece))
                return true;

            if ((piece.First() == ' ' && word.StartsWith(piece[1..], StringComparison.Ordinal))
                || (piece.Last() == ' ' && word.EndsWith(piece[0..^1], StringComparison.Ordinal)))
                return true;
        }

        return false;
    }
}
