using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using TypingRealm.Library.Books;
using TypingRealm.Library.Infrastructure.DataAccess.Entities;
using TypingRealm.Library.Sentences;

namespace TypingRealm.Library.Infrastructure.DataAccess.Repositories;

public sealed record WordWithRowId(
    Word Word, long RowId);

public sealed class SentenceRepository : ISentenceRepository
{
    private readonly LibraryDbContext _dbContext;
    private readonly ILogger<SentenceRepository> _logger;

    public SentenceRepository(
        LibraryDbContext dbContext,
        ILogger<SentenceRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public ValueTask<SentenceId> NextIdAsync() => new(new SentenceId(Guid.NewGuid().ToString()));

    public async ValueTask RemoveAllForBook(BookId bookId)
    {
        using var connection = new NpgsqlConnection(_dbContext.Database.GetConnectionString() + "; Command Timeout = 240;");
        await connection.OpenAsync()
            .ConfigureAwait(false);

        using var command = new NpgsqlCommand("DELETE FROM sentence WHERE book_id = (@book_id)", connection)
        {
            Parameters =
            {
                new("book_id", bookId.Value)
            }
        };
        await command.ExecuteNonQueryAsync()
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
        IEnumerable<Sentence> allSentences)
    {
        //var batchSize = 200; // For EF, it allocates a lot of RAM.
        var batchSize = 3000;

        var chunks = allSentences.Chunk(batchSize);

        using var connection = new NpgsqlConnection(
            _dbContext.Database.GetConnectionString() + "; Include Error Detail=true");

        await connection.OpenAsync()
            .ConfigureAwait(false);

        var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable)
            .ConfigureAwait(false);

        var lastWord = await _dbContext.Word
            .OrderByDescending(w => w.Id)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        var startWordId = lastWord == null ? 1 : lastWord.Id + 1;

        var sw = new Stopwatch();
        foreach (var chunk in chunks)
        {
            sw.Reset();
            sw.Start();
            startWordId = SaveBulkFast(chunk, connection, startWordId);
            startWordId++;
            sw.Stop();

            _logger.LogDebug("Successfully inserted a chunk of sentences, took {ChunkTookMs} ms.", sw.ElapsedMilliseconds);
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

        await context.SaveChangesAsync()
            .ConfigureAwait(false);
    }

    private long SaveBulkFast(
        IEnumerable<Sentence> sentencesBatch,
        NpgsqlConnection connection,
        long startWordId)
    {
        var sentences = sentencesBatch.ToDictionary(x => x.SentenceId);

        var wordsWithRowId = sentences.Values
            .SelectMany(x => x.Words)
            .OrderBy(x => sentences[x.SentenceId].IndexInBook)
            .ThenBy(x => x.IndexInSentence)
            .Select((word, index) => new WordWithRowId(word, startWordId + index))
            .ToList();

        using (var sentenceWriter = connection.BeginBinaryImport("COPY sentence (id, book_id, index_in_book, value) FROM STDIN (FORMAT BINARY)"))
        {
            // Consider using Async api.
            foreach (var sentence in sentences.Values)
            {
                sentenceWriter.StartRow();
                sentenceWriter.Write(sentence.SentenceId.Value, NpgsqlDbType.Varchar);
                sentenceWriter.Write(sentence.BookId.Value, NpgsqlDbType.Varchar);
                sentenceWriter.Write(sentence.IndexInBook, NpgsqlDbType.Integer);
                sentenceWriter.Write(sentence.Value, NpgsqlDbType.Text);
            }

            sentenceWriter.Complete();
        }

        using (var wordWriter = connection.BeginBinaryImport("COPY word (id, sentence_id, index_in_sentence, value, raw_value, count_in_sentence, raw_count_in_sentence) FROM STDIN (FORMAT BINARY)"))
        {
            foreach (var wordWithRowId in wordsWithRowId)
            {
                var word = wordWithRowId.Word;

                wordWriter.StartRow();
                wordWriter.Write(wordWithRowId.RowId, NpgsqlDbType.Bigint);
                wordWriter.Write(word.SentenceId.Value, NpgsqlDbType.Varchar);
                wordWriter.Write(word.IndexInSentence, NpgsqlDbType.Integer);
                wordWriter.Write(word.Value, NpgsqlDbType.Varchar);
                wordWriter.Write(word.RawValue, NpgsqlDbType.Varchar);
                wordWriter.Write(word.CountInSentence, NpgsqlDbType.Integer);
                wordWriter.Write(word.RawCountInSentence, NpgsqlDbType.Integer);
            }

            wordWriter.Complete();
        }

        using var keyPairWriter = connection.BeginBinaryImport("COPY key_pair (index_in_word, value, count_in_word, count_in_sentence, word_id) FROM STDIN (FORMAT BINARY)");

        foreach (var keyPairWithWordRowId in wordsWithRowId.SelectMany(x => x.Word.KeyPairs.Select(kp => new
        {
            WordRowId = x.RowId,
            KeyPair = kp
        })))
        {
            var keyPair = keyPairWithWordRowId.KeyPair;

            keyPairWriter.StartRow();
            keyPairWriter.Write(keyPair.IndexInWord, NpgsqlDbType.Integer);
            keyPairWriter.Write(keyPair.Value, NpgsqlDbType.Varchar);
            keyPairWriter.Write(keyPair.CountInWord, NpgsqlDbType.Integer);
            keyPairWriter.Write(keyPair.CountInSentence, NpgsqlDbType.Integer);
            keyPairWriter.Write(keyPairWithWordRowId.WordRowId, NpgsqlDbType.Bigint);
        }

        keyPairWriter.Complete();

        var lastWord = wordsWithRowId[^1];
        return lastWord.RowId;
    }
}
