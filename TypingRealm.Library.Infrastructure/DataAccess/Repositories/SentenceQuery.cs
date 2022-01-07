﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

    public async ValueTask<IEnumerable<SentenceDto>> FindSentencesContainingKeyPairsAsync(IEnumerable<string> keyPairs, int sentencesCount)
    {
        var sentences = await _dbContext.KeyPair
            .Where(keyPair => keyPairs.Contains(keyPair.Value))
            .Include(keyPair => keyPair.Word)
            .ThenInclude(word => word.Sentence)
            .OrderByDescending(keyPair => keyPair.CountInSentence * keyPair.Word.CountInSentence)
            .Select(x => new
            {
                SentenceId = x.Word.SentenceId,
                SentenceValue = x.Word.Sentence.Value,
                CountInWord = x.CountInWord,
                CountInSentence = x.CountInSentence
            })
            .Take(sentencesCount)
            .ToListAsync()
            .ConfigureAwait(false);

        var result = sentences
            .Select(x => new SentenceDto(x.SentenceId, x.SentenceValue))
            .ToList();

        return result;
    }

    public async ValueTask<IEnumerable<SentenceDto>> FindSentencesContainingWordsAsync(IEnumerable<string> words, int sentencesCount)
    {
        var searchWords = words.Select(word => word.ToLowerInvariant()).ToList();

        var sentences = await _dbContext.Word
            .Include(word => word.Sentence)
            .Where(word => searchWords.Contains(word.RawValue))
            .OrderByDescending(word => word.RawCountInSentence)
            .Select(x => new
            {
                SentenceId = x.SentenceId,
                SentenceValue = x.Sentence.Value,
                CountInSentence = x.CountInSentence
            })
            .Take(sentencesCount)
            .ToListAsync()
            .ConfigureAwait(false);

        var result = sentences
            .Select(x => new SentenceDto(x.SentenceId, x.SentenceValue))
            .ToList();

        return result;
    }
}
