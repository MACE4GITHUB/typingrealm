using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Library.Infrastructure.DataAccess.Entities;

namespace TypingRealm.Library.Infrastructure.DataAccess.Repositories;

public sealed class SentenceQuery : ISentenceQuery
{
    private readonly LibraryDbContext _dbContext;

    public SentenceQuery(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<IEnumerable<SentenceDto>> FindRandomSentencesAsync(int sentencesCount, int consecutiveSentencesCount)
    {
        var allBooks = await _dbContext.Book
            .Where(x => !x.IsArchived && x.IsProcessed)
            .Select(x => new { BookId = x.Id })
            .ToListAsync()
            .ConfigureAwait(false);

        var randomBooks = Enumerable.Range(0, sentencesCount)
            .Select(_ => RandomNumberGenerator.GetInt32(0, allBooks.Count))
            .Select(index => allBooks[index])
            .ToList();

        var booksIds = randomBooks.Select(x => x.BookId)
            .Distinct()
            .ToList();

        var maxSentenceIndexInBook = await _dbContext.Sentence
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

        var sentences = await _dbContext.Sentence.Where(
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
            .Select(x => new SentenceDto(x.SentenceId, x.Value))
            .ToList();

        return result;
    }

    public sealed record BookAndSentencePair(string BookId, int SentenceIndexInBook)
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

    public ValueTask<IEnumerable<SentenceDto>> FindSentencesContainingKeyPairsAsync(IEnumerable<string> keyPairs, int sentencesCount)
    {
        throw new NotImplementedException();
    }

    public ValueTask<IEnumerable<SentenceDto>> FindSentencesContainingWordsAsync(IEnumerable<string> words, int sentencesCount)
    {
        throw new NotImplementedException();
    }
}

public sealed class SentenceRepository : ISentenceRepository
{
    private readonly LibraryDbContext _dbContext;

    public SentenceRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ValueTask<SentenceId> NextIdAsync() => new(new SentenceId(Guid.NewGuid().ToString()));

    // TODO: Fix the implementation of SaveAsync and SaveBulkAsync to potentially be able to update records?
    // (right now it is only working for appending).
    public async ValueTask SaveAsync(Sentence sentence)
    {
        var dao = SentenceDao.ToDao(sentence);

        await _dbContext.Sentence.AddAsync(dao)
            .ConfigureAwait(false);

        await _dbContext.SaveChangesAsync()
            .ConfigureAwait(false);
    }

    public async ValueTask SaveByBatchesAsync(
        IEnumerable<Sentence> allSentences,
        int batchSize)
    {
        var chunks = allSentences.Chunk(batchSize);

        using var connection = new Npgsql.NpgsqlConnection(
            _dbContext.Database.GetConnectionString());

        await connection.OpenAsync()
            .ConfigureAwait(false);

        var transaction = await connection.BeginTransactionAsync()
            .ConfigureAwait(false);

        foreach (var chunk in chunks)
        {
            await SaveBulkAsync(chunk, connection, transaction)
                .ConfigureAwait(false);
        }

        await transaction.CommitAsync()
            .ConfigureAwait(false);
    }

    private async ValueTask SaveBulkAsync(
        IEnumerable<Sentence> sentencesBatch,
        DbConnection transactionConnection,
        DbTransaction transaction)
    {
        // TODO: Centralize this registration.
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseNpgsql(transactionConnection)
            .UseSnakeCaseNamingConvention()
            .Options;

        using var context = new LibraryDbContext(options);
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        await context.Database.UseTransactionAsync(transaction)
            .ConfigureAwait(false);

        await context.Sentence.AddRangeAsync(sentencesBatch.Select(s => SentenceDao.ToDao(s)))
            .ConfigureAwait(false);

        try
        {
            await context.SaveChangesAsync()
                .ConfigureAwait(false);
        }
        catch (Exception exc)
        {
            throw;
        }
    }
}
