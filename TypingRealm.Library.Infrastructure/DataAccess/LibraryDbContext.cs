using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Library.Infrastructure.DataAccess.Entities;

namespace TypingRealm.Library.Infrastructure.DataAccess;

#pragma warning disable CS8618
public sealed class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    public DbSet<BookDao> Book { get; set; }
    public DbSet<BookContentDao> BookContent { get; set; }

    public DbSet<SentenceDao> Sentence { get; set; }
    public DbSet<WordDao> Word { get; set; }
    public DbSet<KeyPairDao> KeyPair { get; set; }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var entities = ChangeTracker.Entries()
            .Where(x => x.Entity is TrackableDao && (x.State == EntityState.Added || x.State == EntityState.Modified));

        // TODO: Get current user context here and also save CreatedBy & UpdatedBy.
        foreach (var entity in entities)
        {
            if (entity.State == EntityState.Added)
            {
                ((TrackableDao)entity.Entity).CreatedAtUtc = now;
            }
            ((TrackableDao)entity.Entity).UpdatedAtUtc = now;
        }

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
#pragma warning restore CS8618
