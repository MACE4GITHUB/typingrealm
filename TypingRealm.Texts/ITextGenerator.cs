using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypingRealm.Texts;

public interface ITextGenerator
{
    ValueTask<string> GenerateTextAsync(TextGenerationConfiguration configuration);
}

public sealed class TextGenerator : ITextGenerator
{
    private const int MaxTextLength = 500;

    private readonly TextRetrieverResolver _textRetrieverResolver;

    public TextGenerator(TextRetrieverResolver textRetrieverResolver)
    {
        _textRetrieverResolver = textRetrieverResolver;
    }

    public static IEnumerable<string> GetSentencesEnumerable(string text)
    {
        return text.Split(". ", StringSplitOptions.RemoveEmptyEntries)
            .Select(text => text.TrimEnd('.'))
            .Select(text => $"{text}.");
    }

    public static IEnumerable<string> GetWordsEnumerable(string text)
    {
        return GetSentencesEnumerable(text)
            .SelectMany(sentence => sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public ValueTask<string> GenerateTextAsync(TextGenerationConfiguration configuration)
    {
        Validate(configuration);

        var textRetriever = _textRetrieverResolver(configuration.Language);

        if (configuration.TextType == TextGenerationType.Text)
            return GenerateTextAsync(textRetriever, configuration.Length);

        if (configuration.TextType == TextGenerationType.Words)
            return GenerateWordsAsync(
                textRetriever,
                configuration.Length,
                configuration.ShouldContain);

        throw new NotSupportedException($"Unknown TextGenerationType: {configuration.TextType}.");
    }

    private static void Validate(TextGenerationConfiguration configuration)
    {
        if (configuration.Length < 0 || configuration.Length > MaxTextLength)
            throw new ArgumentException($"Invalid configuration: length should be positive number below or equal to {MaxTextLength}.");
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

    private async ValueTask<string> GenerateTextAsync(
        ITextRetriever textRetriever, int length)
    {
        var builder = new StringBuilder();

        while (builder.Length < Math.Min(length, MaxTextLength))
        {
            var text = await textRetriever.RetrieveTextAsync()
                .ConfigureAwait(false);

            var sentences = GetSentencesEnumerable(text);

            foreach (var sentence in sentences)
            {
                if (builder.Length == 0)
                {
                    builder.Append(sentence);
                }
                else
                {
                    builder.Append($" {sentence}");
                }

                if (builder.Length >= Math.Min(length, MaxTextLength))
                    break;
            }
        }

        return builder.ToString();
    }

    private async ValueTask<string> GenerateWordsAsync(
        ITextRetriever textRetriever, int length, IEnumerable<string> shouldContain)
    {
        var builder = new StringBuilder();

        var maxTries = 200; // Avoid endless loops because of shouldContain logic.
        var tries = 0;

        while (builder.Length < Math.Min(length, MaxTextLength))
        {
            var text = await textRetriever.RetrieveTextAsync()
                .ConfigureAwait(false);

            var words = GetWordsEnumerable(text);

            tries++;
            foreach (var word in words)
            {
                if (!IsAllowed(word, shouldContain) && tries < maxTries)
                    continue;

                if (builder.Length == 0)
                {
                    builder.Append(word);
                }
                else
                {
                    builder.Append($" {word}");
                }

                if (builder.Length >= Math.Min(length, MaxTextLength))
                    break;
            }
        }

        return builder.ToString();
    }
}
