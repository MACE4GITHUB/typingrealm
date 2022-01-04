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
    public DbSet<SentenceDao> Sentence { get; set; }
    public DbSet<WordDao> Word { get; set; }
    public DbSet<KeyPairDao> KeyPair { get; set; }
}
#pragma warning restore CS8618
