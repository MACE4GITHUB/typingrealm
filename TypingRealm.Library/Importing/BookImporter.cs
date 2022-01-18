using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TypingRealm.Library.Books;
using TypingRealm.Library.Sentences;
using TypingRealm.TextProcessing;

namespace TypingRealm.Library.Importing;

public interface IBookImporter
{
    ValueTask<BookImportResult> ImportBookAsync(BookId bookId);
}

public sealed class BookImporter : IBookImporter
{
    private static readonly int _minSentenceLengthCharacters = 8;
    private readonly IBookRepository _bookStore;
    private readonly ISentenceRepository _sentenceRepository;
    private readonly ITextProcessor _textProcessor;
    private readonly ILanguageProvider _languageProvider;
    private readonly ILogger<BookImporter> _logger;

    public BookImporter(
        IBookRepository bookStore,
        ISentenceRepository sentenceRepository,
        ITextProcessor textProcessor,
        ILanguageProvider languageProvider,
        ILogger<BookImporter> logger)
    {
        _bookStore = bookStore;
        _sentenceRepository = sentenceRepository;
        _textProcessor = textProcessor;
        _languageProvider = languageProvider;
        _logger = logger;
    }

    public async ValueTask<BookImportResult> ImportBookAsync(BookId bookId)
    {
        var book = await _bookStore.FindBookAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book {bookId} is not found.");

        var bookContent = await _bookStore.FindBookContentAsync(bookId)
            .ConfigureAwait(false);

        if (bookContent == null)
            throw new InvalidOperationException($"Content for the book {bookId} is not found.");

        book.StartProcessing();

        await _bookStore.UpdateBookAsync(book)
            .ConfigureAwait(false);

        await _sentenceRepository.RemoveAllForBook(bookId)
            .ConfigureAwait(false);

        try
        {
            var result = await ImportBookAsync(book, bookContent)
                .ConfigureAwait(false);

            book.FinishProcessing();

            await _bookStore.UpdateBookAsync(book)
                .ConfigureAwait(false);

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while importing the book {BookId}.", bookId.Value);
            book.ErrorProcessing();

            await _bookStore.UpdateBookAsync(book)
                .ConfigureAwait(false);

            throw new InvalidOperationException($"Error while importing book {bookId}");
        }
    }

    private async ValueTask<BookImportResult> ImportBookAsync(Book book, BookContent bookContent)
    {
        using var reader = new StreamReader(bookContent.Content);
        var text = await reader.ReadToEndAsync()
            .ConfigureAwait(false);

        var languageInfo = await _languageProvider.FindLanguageInformationAsync(book.Language)
            .ConfigureAwait(false);

        var tooShortSentences = _textProcessor.GetSentencesEnumerable(text)
            .Where(sentence => sentence.Length < _minSentenceLengthCharacters)
            .Distinct()
            .ToList();

        var notAllowedSentences = _textProcessor.GetSentencesEnumerable(text)
            .Where(sentence => !languageInfo.IsAllLettersAllowed(sentence))
            .Distinct()
            .ToList();

        var notAllowedCharacters = notAllowedSentences.SelectMany(
            sentence => sentence.Where(character => !languageInfo.IsAllLettersAllowed(character.ToString())))
            .Distinct()
            .ToList();

        var sentencesEnumerable = _textProcessor.GetSentencesEnumerable(text, languageInfo)
            .Where(sentence => sentence.Length >= _minSentenceLengthCharacters)
            .Select((sentence, sentenceIndex) => CreateSentence(book.BookId, sentence, sentenceIndex));

        await _sentenceRepository.SaveByBatchesAsync(sentencesEnumerable)
            .ConfigureAwait(false);

        return new BookImportResult(
            book,
            tooShortSentences,
            notAllowedSentences,
            string.Join(string.Empty, notAllowedCharacters));
    }

    private Sentence CreateSentence(BookId bookId, string sentence, int sentenceIndex)
    {
        var sentenceId = SentenceId.New();
        var words = _textProcessor.GetWordsEnumerable(sentence).ToArray();
        var wordsList = new List<Word>(words.Length);

        var wordsInSentence = words
            .GroupBy(word => word)
            .Select(group => new
            {
                Word = group.Key,
                Count = group.Count()
            }).ToDictionary(x => x.Word);

        var rawWordsInSentence = words
            .Select(word => _textProcessor.NormalizeWord(word))
            .GroupBy(word => word)
            .Select(group => new
            {
                Word = group.Key,
                Count = group.Count()
            }).ToDictionary(x => x.Word);

        var index = 0;
        foreach (var word in words)
        {
            var keyPairs = CreateKeyPairs(sentence, word);

            var rawWord = _textProcessor.NormalizeWord(word);
            wordsList.Add(new Word(
                sentenceId, index,
                word, rawWord,
                wordsInSentence[word].Count, rawWordsInSentence[rawWord].Count,
                keyPairs));
            index++;
        }

        return new Sentence(bookId, sentenceId, sentenceIndex, sentence, wordsList);
    }

    private static IList<KeyPair> CreateKeyPairs(string sentence, string word)
    {
        // TODO: Measure performance here (ToDictionary).
        var keyPairsInSentence = GetKeyPairsEnumerable(sentence)
            .GroupBy(keyPair => keyPair.KeyPair)
            .Select(group => new
            {
                KeyPair = group.Key,
                Count = group.Count()
            }).ToDictionary(x => x.KeyPair);

        var keyPairs = GetKeyPairsEnumerable(word).ToList();

        var keyPairsInWord = keyPairs
            .GroupBy(keyPair => keyPair.KeyPair)
            .Select(group => new
            {
                KeyPair = group.Key,
                Count = group.Count()
            }).ToDictionary(x => x.KeyPair);

        return keyPairs.Select(x => new KeyPair(
            x.Index,
            x.KeyPair,
            keyPairsInWord[x.KeyPair].Count,
            keyPairsInSentence[x.KeyPair].Count)).ToList();
    }

    private sealed record KeyPairInText(int Index, string KeyPair);
    private static IEnumerable<KeyPairInText> GetKeyPairsEnumerable(string value)
    {
        if (value.Length == 0)
            throw new ArgumentException("Value should not be empty.", nameof(value));

        yield return new KeyPairInText(-1, $" {value[0]}");

        if (value.Length > 1)
            yield return new KeyPairInText(-1, $" {value[..2]}");

        var index = 0;
        while (index < value.Length - 1)
        {
            yield return new KeyPairInText(index, value.Substring(index, 2));

            if (value[index..].Length > 2)
                yield return new KeyPairInText(index, value.Substring(index, 3));
            else
                yield return new KeyPairInText(index, $"{value.Substring(index, 2)} ");

            index++;
        }

        yield return new KeyPairInText(index, $"{value[^1]} ");
    }
}
