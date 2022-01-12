using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Library.Infrastructure.DataAccess.Entities;

namespace TypingRealm.Library.Infrastructure.DataAccess.Repositories;

public sealed class SentenceRepository : ISentenceRepository
{
    private readonly LibraryDbContext _dbContext;

    public SentenceRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ValueTask<SentenceId> NextIdAsync() => new(new SentenceId(Guid.NewGuid().ToString()));

    public async ValueTask RemoveAllForBook(BookId bookId)
    {
        var sentences = await _dbContext.Sentence
            .Where(sentence => sentence.BookId == bookId.Value)
            .ToListAsync()
            .ConfigureAwait(false);

        _dbContext.Sentence.RemoveRange(sentences);

        await _dbContext.SaveChangesAsync()
            .ConfigureAwait(false);
    }

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
