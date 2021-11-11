using System.Threading.Tasks;

namespace TypingRealm.Typing
{
    public interface ITypingSessionRepository
    {
        ValueTask<TypingSession?> FindAsync(string typingSessionId);
        ValueTask SaveAsync(TypingSession typingSession);
        ValueTask<string> NextIdAsync();
    }
}
