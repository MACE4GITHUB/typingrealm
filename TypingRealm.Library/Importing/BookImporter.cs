using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TypingRealm.Library.Books;
using TypingRealm.Library.Sentences;
using TypingRealm.Texts;

namespace TypingRealm.Library.Importing;

public interface IBookImporter
{
    ValueTask<BookImportResult> ImportNewBookAsync(string description, string language, Stream content);
    ValueTask<BookImportResult> ReImportBookAsync(BookId bookId);
}

public sealed class BookImporter : IBookImporter
{
    private readonly IBookRepository _bookStore;
    private readonly ISentenceRepository _sentenceRepository;

    public BookImporter(
        IBookRepository bookStore,
        ISentenceRepository sentenceRepository)
    {
        _bookStore = bookStore;
        _sentenceRepository = sentenceRepository;
    }

    public async ValueTask<BookImportResult> ImportNewBookAsync(string description, string language, Stream content)
    {
        var bookId = await _bookStore.NextBookIdAsync()
            .ConfigureAwait(false);

        var bookContent = new BookContent(bookId, content);
        var book = new Book(bookId, new(language), new(description));

        await _bookStore.AddBookWithContentAsync(book, bookContent)
            .ConfigureAwait(false);

        // TODO: Do the following part asynchronously, return statistics.

        bookContent = await _bookStore.FindBookContentAsync(bookId)
            .ConfigureAwait(false);
        if (bookContent == null)
            throw new InvalidOperationException("Book content has not been found.");

        var importResult = await ImportBookAsync(book, bookContent)
            .ConfigureAwait(false);

        book = await _bookStore.FindBookAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException("Did not find the book.");

        book.FinishProcessing();

        await _bookStore.UpdateBookAsync(book)
            .ConfigureAwait(false);

        return importResult;
    }

    public async ValueTask<BookImportResult> ReImportBookAsync(BookId bookId)
    {
        var book = await _bookStore.FindBookAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException("Book is not found.");

        var bookContent = await _bookStore.FindBookContentAsync(bookId)
            .ConfigureAwait(false);

        if (bookContent == null)
            throw new InvalidOperationException("Content for a book is not found.");

        if (book.IsProcessed)
            book.StartReprocessing();

        await _bookStore.UpdateBookAsync(book)
            .ConfigureAwait(false);

        await _sentenceRepository.RemoveAllForBook(bookId)
            .ConfigureAwait(false);

        var result = await ImportBookAsync(book, bookContent)
            .ConfigureAwait(false);

        // TODO: Reuse this code.

        book.FinishProcessing();

        await _bookStore.UpdateBookAsync(book)
            .ConfigureAwait(false);

        return result;
    }

    private async ValueTask<BookImportResult> ImportBookAsync(Book book, BookContent bookContent)
    {
        // TODO: Parse the stream by chunks.

        using var reader = new StreamReader(bookContent.Content);
        var text = await reader.ReadToEndAsync()
            .ConfigureAwait(false);

        var notAllowedSentences = TextHelpers.GetDisallowedSentencesEnumerable(text, book.Language)
            .ToList();

        var notAllowedCharacters = notAllowedSentences.SelectMany(
            sentence => sentence.Where(character => !TextHelpers.IsAllLettersAllowed(character.ToString(), book.Language)))
            .Distinct()
            .ToList();

        // TODO: Move most of this logic inside GetSentencesEnumerable method.
        var sentences = TextHelpers.GetAllowedSentencesEnumerable(text, book.Language)
            .Select((sentence, sentenceIndex) => CreateSentence(book.BookId, sentence, sentenceIndex));

        await _sentenceRepository.SaveByBatchesAsync(sentences, 200)
            .ConfigureAwait(false);

        return new BookImportResult(book, notAllowedSentences.Take(10).ToList(), string.Join(string.Empty, notAllowedCharacters));
    }

    private Sentence CreateSentence(BookId bookId, string sentence, int sentenceIndex)
    {
        var sentenceId = SentenceId.New();
        var words = TextHelpers.SplitTextBySpaces(sentence);
        var wordsList = new List<Word>(words.Length);

        var wordsInSentence = words
            .GroupBy(word => word)
            .Select(group => new
            {
                Word = group.Key,
                Count = group.Count()
            }).ToDictionary(x => x.Word);

        var rawWordsInSentence = words
            .Select(word => TextHelpers.GetRawWord(word))
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

            var rawWord = TextHelpers.GetRawWord(word);
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

        // TODO: Figure out whether "index" value is correct here or I need to do +1.
        yield return new KeyPairInText(index, $"{value[^1]} ");
    }
}
