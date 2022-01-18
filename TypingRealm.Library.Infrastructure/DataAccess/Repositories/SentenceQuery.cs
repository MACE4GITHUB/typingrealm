using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Library.Books;
using TypingRealm.Library.Sentences;
using TypingRealm.TextProcessing;

namespace TypingRealm.Library.Infrastructure.DataAccess.Repositories;

public sealed class SentenceQuery : ISentenceQuery
{
    private readonly LibraryDbContext _dbContext;
    private readonly ITextProcessor _textProcessor;
    private readonly string _language;

    public SentenceQuery(
        LibraryDbContext dbContext,
        ITextProcessor textProcessor,
        string language)
    {
        _dbContext = dbContext;
        _textProcessor = textProcessor;
        _language = language;
    }

    public ValueTask<IEnumerable<SentenceDto>> FindSentencesAsync(SentencesRequest request)
    {
        if (!request.IsValid())
            throw new InvalidOperationException("Invalid request.");

        if (request.Type == SentencesRequestType.Random)
            return FindRandomSentencesAsync(request.MaxCount, request.ConsecutiveCount);

        if (request.Type == SentencesRequestType.ContainingWords)
            return FindSentencesContainingWordsAsync(request.Contains, request.MaxCount);

        if (request.Type == SentencesRequestType.ContainingKeyPairs)
            return FindSentencesContainingKeyPairsAsync(request.Contains, request.MaxCount);

        throw new NotSupportedException("Request type is not supported.");
    }

    public ValueTask<IEnumerable<string>> FindWordsAsync(WordsRequest request)
    {
        if (!request.IsValid())
            throw new InvalidOperationException("Invalid request.");

        if (request.Type == WordsRequestType.Random)
            return FindRandomWordsAsync(request.MaxCount, request.RawWords);

        if (request.Type == WordsRequestType.ContainingKeyPairs)
            return FindWordsContainingKeyPairsAsync(request.Contains, request.MaxCount, request.RawWords);

        throw new NotSupportedException("Request type is not supported.");
    }

    private async ValueTask<IEnumerable<string>> FindRandomWordsAsync(int maxCount, bool rawWords)
    {
        var randomSentences = await FindRandomSentencesAsync(maxCount, 1)
            .ConfigureAwait(false);

        var wordsEnumerable = randomSentences.SelectMany(sentence => _textProcessor.GetWordsEnumerable(sentence.Value));

        if (rawWords)
            wordsEnumerable = wordsEnumerable.Select(word => _textProcessor.NormalizeWord(word));

        var distinctWords = wordsEnumerable.Distinct().ToList();

        return Enumerable.Range(0, maxCount)
            .Select(index => RandomNumberGenerator.GetInt32(0, distinctWords.Count))
            .Select(index => distinctWords[index])
            .ToList();
    }

    private async ValueTask<IEnumerable<SentenceDto>> FindRandomSentencesAsync(int maxSentencesCount, int consecutiveSentencesCount)
    {
        var allBooks = await _dbContext.Book.AsNoTracking()
            .Where(x => !x.IsArchived && x.ProcessingStatus == ProcessingStatus.Processed && x.Language == _language)
            .Select(x => new { BookId = x.Id })
            .ToListAsync()
            .ConfigureAwait(false);

        var amountOfRecords = (maxSentencesCount * 2 / consecutiveSentencesCount) +1;

        var randomBooks = Enumerable.Range(0, amountOfRecords)
            .Select(_ => RandomNumberGenerator.GetInt32(0, allBooks.Count))
            .Select(index => allBooks[index])
            .ToList();

        var booksIds = randomBooks.Select(x => x.BookId)
            .Distinct()
            .ToList();

        var maxSentenceIndexInBook = await _dbContext.Sentence.AsNoTracking()
            .Where(x => booksIds.Contains(x.BookId))
            .GroupBy(x => x.BookId)
            .Select(group => group.OrderByDescending(x => x.IndexInBook).First())
            .ToListAsync()
            .ConfigureAwait(false);

        var maxSentenceIndexInBookDict = maxSentenceIndexInBook.ToDictionary(x => x!.BookId);

        var sentencesInfo = randomBooks.SelectMany(book =>
        {
            var currentMaxSentenceIndexInBook = maxSentenceIndexInBookDict[book.BookId]!.IndexInBook;

            var randomSentenceIndexInBook = RandomNumberGenerator.GetInt32(
                0, currentMaxSentenceIndexInBook);

            return GenerateBookAndSentencePairs(book.BookId, randomSentenceIndexInBook, currentMaxSentenceIndexInBook, consecutiveSentencesCount);
        }).Select((x, index) =>
        {
            x.Index = index;
            return x;
        }).ToList();

        var localIdPairs = sentencesInfo
            .Select(x => $"{x.BookId}-{x.SentenceIndexInBook}")
            .ToList();

        var sentences = await _dbContext.Sentence.AsNoTracking().Where(
            x => localIdPairs.Any(localId => localId == x.BookId + "-" + x.IndexInBook))
            .Select(x => new { SentenceId = x.Id, Value = x.Value, IndexInBook = x.IndexInBook, BookId = x.BookId })
            .ToListAsync()
            .ConfigureAwait(false);

        var joinResult =
            from info in sentencesInfo
            join sentence in sentences
            on new { info.BookId, IndexInBook = info.SentenceIndexInBook } equals new { sentence.BookId, sentence.IndexInBook }
            select new { info.Index, info.BookId, info.SentenceIndexInBook, sentence.SentenceId, sentence.Value };

        var result = joinResult
            .OrderBy(x => x.Index)
            .Take(maxSentencesCount)
            .Select(x => new SentenceDto(x.SentenceId, x.Value))
            .ToList();

        return result;
    }

    private sealed record BookAndSentencePair(string BookId, int SentenceIndexInBook)
    {
        public int Index { get; set; }
    }

    private static IEnumerable<BookAndSentencePair> GenerateBookAndSentencePairs(
        string bookId, int startSentenceIndexInBook, int maxSentenceIndexInBook, int consecutiveSentencesCount)
    {
        var count = Math.Min(maxSentenceIndexInBook - startSentenceIndexInBook + 1, consecutiveSentencesCount);

        return Enumerable.Range(startSentenceIndexInBook, count)
            .Select(indexInBook => new BookAndSentencePair(bookId, indexInBook));
    }

    private async ValueTask<IEnumerable<SentenceDto>> FindSentencesContainingKeyPairsAsync(IEnumerable<string> keyPairs, int maxSentencesCount)
    {
        var sentences = await _dbContext.KeyPair.AsNoTracking()
            .Where(keyPair => keyPairs.Contains(keyPair.Value))
            .Include(keyPair => keyPair.Word)
            .ThenInclude(word => word.Sentence)
            .ThenInclude(sentence => sentence.Book)
            .Where(kp => kp.Word.Sentence.Book.Language == _language)
            .OrderByDescending(keyPair => keyPair.CountInSentence)
            .Select(x => new
            {
                KeyPair = x.Value,
                SentenceId = x.Word.SentenceId,
                SentenceValue = x.Word.Sentence.Value,
                CountInWord = x.CountInWord,
                CountInSentence = x.CountInSentence
            })
            .Take(maxSentencesCount * 5)
            .ToListAsync()
            .ConfigureAwait(false);

        var result = sentences
            .DistinctBy(x => x.SentenceId)
            .OrderByDescending(x => x.CountInSentence)
            .Select(x => new SentenceDto(x.SentenceId, x.SentenceValue))
            .Take(maxSentencesCount)
            .ToList();

        return result;
    }

    private async ValueTask<IEnumerable<SentenceDto>> FindSentencesContainingWordsAsync(IEnumerable<string> words, int maxSentencesCount)
    {
        var searchWords = words.Select(word => word.ToLowerInvariant()).ToList();

        var sentences = await _dbContext.Word.AsNoTracking()
            .Where(word => searchWords.Contains(word.RawValue))
            .Include(word => word.Sentence)
            .ThenInclude(sentence => sentence.Book)
            .Where(word => word.Sentence.Book.Language == _language)
            .OrderByDescending(word => word.RawCountInSentence)
            .Select(x => new
            {
                SentenceId = x.SentenceId,
                SentenceValue = x.Sentence.Value,
                RawCountInSentence = x.RawCountInSentence
            })
            .Take(maxSentencesCount * 5)
            .ToListAsync()
            .ConfigureAwait(false);

        var result = sentences
            .DistinctBy(x => x.SentenceId)
            .OrderByDescending(x => x.RawCountInSentence)
            .Select(x => new SentenceDto(x.SentenceId, x.SentenceValue))
            .Take(maxSentencesCount)
            .ToList();

        return result;
    }

    private async ValueTask<IEnumerable<string>> FindWordsContainingKeyPairsAsync(
        IEnumerable<string> keyPairs, int maxWordsCount, bool rawWords)
    {
        if (maxWordsCount <= 0)
            throw new ArgumentException("Count should be at least 1 or higher.", nameof(maxWordsCount));

        if (!keyPairs.Any())
            throw new ArgumentException("KeyPairs cannot be empty.", nameof(keyPairs));

        if (rawWords)
        {
            keyPairs = keyPairs
                .Where(kp => IsKeyPairWithoutPunctuation(kp))
                .Select(kp => kp.ToLowerInvariant())
                .ToList();
        }

        var words = await _dbContext.KeyPair.AsNoTracking()
            .Where(keyPair => keyPairs.Contains(keyPair.Value))
            .Include(keyPair => keyPair.Word)
            .ThenInclude(word => word.Sentence)
            .ThenInclude(sentence => sentence.Book)
            .Where(kp => kp.Word.Sentence.Book.Language == _language)
            .Select(x => rawWords ? x.Word.RawValue : x.Word.Value)
            .Distinct()
            .Take(maxWordsCount)
            .ToListAsync()
            .ConfigureAwait(false);

        return words;
    }

    private bool IsKeyPairWithoutPunctuation(string keyPair)
    {
        foreach (var punctuationCharacter in TextConstants.PunctuationCharacters)
        {
            if (keyPair.StartsWith(punctuationCharacter) || keyPair.EndsWith(punctuationCharacter))
                return false;
        }

        return true;
    }
}
