using Microsoft.EntityFrameworkCore;
using TypingRealm.Data.Infrastructure.DataAccess.Entities;

namespace TypingRealm.Data.Infrastructure.DataAccess
{
#pragma warning disable CS8618
    public sealed class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Text> Text { get; set; }
        public DbSet<TypingSession> TypingSession { get; set; }
        public DbSet<UserSession> UserSession { get; set; }

        /*public DbSet<TextTypingResult> TextTypingResult { get; set; }
        public DbSet<KeyPressEvent> KeyPressEvent { get; set; }
        public DbSet<TypingSessionText> TypingSessionText { get; set; }*/
    }
#pragma warning restore CS8618
}
