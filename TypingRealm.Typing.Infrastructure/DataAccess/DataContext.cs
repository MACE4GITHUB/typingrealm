using Microsoft.EntityFrameworkCore;

namespace TypingRealm.Typing.Infrastructure.DataAccess;

#pragma warning disable CS8618
public sealed class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Entities.Text> Text { get; set; }
    public DbSet<Entities.TypingSession> TypingSession { get; set; }
    public DbSet<Entities.UserSession> UserSession { get; set; }

    /*public DbSet<TextTypingResult> TextTypingResult { get; set; }
    public DbSet<KeyPressEvent> KeyPressEvent { get; set; }
    public DbSet<TypingSessionText> TypingSessionText { get; set; }*/
}
#pragma warning restore CS8618
