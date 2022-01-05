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
        var bulk = new List<Sentence>(100);

        var sentenceIndex = 0;
        foreach (var sentenceValue in TextHelpers.GetSentencesEnumerable(book.Content))
        {
            if (!TextHelpers.IsAllLettersAllowed(sentenceValue, "en")) // TODO: Support other languages.
                continue;

            if (sentenceValue.Length < TextHelpers.MinSentenceLength)
                continue;

            var sentence = await CreateSentenceAsync(book.BookId, sentenceValue, sentenceIndex)
                .ConfigureAwait(false);

            if (bulk.Count < 100)
                bulk.Add(sentence);
            else
            {
                await _sentenceRepository.SaveBulkAsync(bulk)
                    .ConfigureAwait(false);

                bulk.Clear();
            }

            sentenceIndex++;
        }

        if (bulk.Count > 0)
        {
            await _sentenceRepository.SaveBulkAsync(bulk)
                .ConfigureAwait(false);

            bulk.Clear();
        }
    }

    private async ValueTask<Sentence> CreateSentenceAsync(BookId bookId, string value, int sentenceIndex)
    {
        var sentenceId = await _sentenceRepository.NextIdAsync()
            .ConfigureAwait(false);

        var words = TextHelpers.GetWordsEnumerable(value)
            .Select((word, index) => new
            {
                Word = word,
                Index = index
            })
            .GroupBy(x => x.Word)
            .Select(group => new
            {
                Word = group.Key,
                Count = group.Count(),
                Values = group
            })
            .SelectMany(x => x.Values.Select(word => new
            {
                Word = word.Word,
                Index = word.Index,
                Count = x.Count
            }))
            .OrderBy(x => x.Index)
            .Select(x => new Word(sentenceId, x.Index, x.Word, x.Count, CreateKeyPairsEnumerable(value, x.Word).ToList()));

        // Let the magic happen.
        // TODO: Measure performance here.
        words = words.ToList();

        return new Sentence(bookId, sentenceId, sentenceIndex, value, words);
    }

    private static IEnumerable<KeyPair> CreateKeyPairsEnumerable(string sentence, string word)
    {
        var wordKeyPairs = GetKeyPairsEnumerable(word)
            .Select((keyPair, index) => new
            {
                KeyPair = keyPair,
                Index = index
            })
            .GroupBy(x => x.KeyPair)
            .Select(group => new
            {
                KeyPair = group.Key,
                Count = group.Count(),
                Values = group
            })
            .SelectMany(x => x.Values.Select(keyPair => new
            {
                KeyPair = keyPair.KeyPair,
                Index = keyPair.Index,
                Count = x.Count
            }))
            .OrderBy(x => x.Index);

        // TODO: Measure performance here (ToDictionary).
        var sentenceKeyPairs = GetKeyPairsEnumerable(sentence)
            .GroupBy(x => x)
            .Select(group => new
            {
                KeyPair = group.Key,
                Count = group.Count()
            }).ToDictionary(x => x.KeyPair);

        return wordKeyPairs.Select(x => new KeyPair(x.Index, x.KeyPair, x.Count, sentenceKeyPairs[x.KeyPair].Count));
    }

    private static IEnumerable<string> GetKeyPairsEnumerable(string value)
    {
        if (value.Length == 0)
            throw new ArgumentException("Value should not be empty.", nameof(value));

        yield return $" {value[0]}";

        var index = 0;
        while (index < value.Length - 1)
        {
            yield return value.Substring(index, 2);

            if (value[index..].Length > 2)
                yield return value.Substring(index, 3);
            else
                yield return $"{value.Substring(index, 2)} ";

            // TODO: !!! IndexInWord is completely incorrect for keypairs.
            // We need to either fix it or it doesn't make sense.

            index++;
        }

        yield return $"{value[^1]} ";
    }
}
