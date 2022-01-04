using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public async ValueTask SaveBulkAsync(IEnumerable<Sentence> sentences)
    {
        await _dbContext.Sentence.AddRangeAsync(sentences.Select(s => SentenceDao.ToDao(s)))
            .ConfigureAwait(false);

        await _dbContext.SaveChangesAsync()
            .ConfigureAwait(false);
    }
}
