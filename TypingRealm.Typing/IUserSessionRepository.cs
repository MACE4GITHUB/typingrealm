using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Typing
{
    public interface IUserSessionRepository
    {
        ValueTask<UserSession?> FindAsync(string userSessionId);
        ValueTask SaveAsync(UserSession userSession);
        ValueTask<string> NextIdAsync();

        IAsyncEnumerable<UserSession> FindAllForUser(string userId);
    }
}
