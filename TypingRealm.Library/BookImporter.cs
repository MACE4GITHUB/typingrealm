using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TypingRealm.Texts;

namespace TypingRealm.Library;

public interface IBookImporter
{
    ValueTask<Book> ImportBookAsync(string description, string content);
}

public sealed class BookImporter : IBookImporter
{
    private readonly ISentenceRepository _sentenceRepository;
    private readonly IBookRepository _bookStore;

    public BookImporter(
        ISentenceRepository sentenceRepository,
        IBookRepository bookStore)
    {
        _sentenceRepository = sentenceRepository;
        _bookStore = bookStore;
    }

    public async ValueTask<Book> ImportBookAsync(string description, string content)
    {
        var bookId = await _bookStore.NextBookIdAsync()
            .ConfigureAwait(false);

        var book = new Book(bookId, description, content);
        await _bookStore.SaveBook(book)
            .ConfigureAwait(false);

        await ImportBookAsync(book)
            .ConfigureAwait(false);

        book = await _bookStore.FindBookAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException("Did not find the book.");

        book.FinishProcessing();

        await _bookStore.SaveBook(book)
            .ConfigureAwait(false);

        return book;
    }

    private async ValueTask ImportBookAsync(Book book)
    {
        // TODO: Move most of this logic inside GetSentencesEnumerable method.
        var sentences = TextHelpers.GetAllowedSentencesEnumerable(book.Content, "en") // TODO: Support other languages.
            .Select((sentence, sentenceIndex) => CreateSentence(book.BookId, sentence, sentenceIndex));

        await _sentenceRepository.SaveByBatchesAsync(sentences, 200)
            .ConfigureAwait(false);
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
