using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Library.Infrastructure.DataAccess.Entities;

namespace TypingRealm.Library.Infrastructure.DataAccess.Repositories;

public sealed class SentenceRepository : ISentenceRepository
{
    private readonly LibraryDbContext _dbContext;

    // TODO: Anti-pattern, don't inject the provider. We need it currently for saving memory.
    private readonly IServiceProvider _provider;

    public SentenceRepository(
        LibraryDbContext dbContext,
        IServiceProvider provider)
    {
        _dbContext = dbContext;
        _provider = provider;
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
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        await context.Sentence.AddRangeAsync(sentences.Select(s => SentenceDao.ToDao(s)))
            .ConfigureAwait(false);

        await context.SaveChangesAsync()
            .ConfigureAwait(false);
    }
}
