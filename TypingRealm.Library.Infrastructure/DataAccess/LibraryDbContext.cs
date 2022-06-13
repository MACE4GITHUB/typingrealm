using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Authentication.Api;
using TypingRealm.Library.Infrastructure.DataAccess.Entities;

namespace TypingRealm.Library.Infrastructure.DataAccess;

#pragma warning disable CS8618
public sealed class LibraryDbContext : DbContext
{
    private readonly IProfileService _profileService;

    public LibraryDbContext(
        DbContextOptions<LibraryDbContext> options,
        IProfileService profileService) : base(options)
    {
        _profileService = profileService;
    }

    public DbSet<BookDao> Book { get; set; }
    public DbSet<BookContentDao> BookContent { get; set; }

    public DbSet<SentenceDao> Sentence { get; set; }
    public DbSet<WordDao> Word { get; set; }
    public DbSet<KeyPairDao> KeyPair { get; set; }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var profile = await _profileService.TryGetProfileAsync()
            .ConfigureAwait(false);

        var entities = ChangeTracker.Entries()
            .Where(x => x.Entity is TrackableDao && (x.State == EntityState.Added || x.State == EntityState.Modified));

        // TODO: Get current user context here and also save CreatedBy & UpdatedBy.
        foreach (var entity in entities)
        {
            var trackableDao = (TrackableDao)entity.Entity;

            if (entity.State == EntityState.Added)
            {
                trackableDao.CreatedAtUtc = now;
                trackableDao.CreatedBy = profile?.ProfileId?.Value;
            }

            trackableDao.UpdatedAtUtc = now;
            trackableDao.UpdatedBy = profile?.ProfileId?.Value;
        }

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken)
            .ConfigureAwait(false);
    }
}
#pragma warning restore CS8618
