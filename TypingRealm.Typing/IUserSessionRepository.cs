using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Typing
{
    public interface IUserSessionRepository
    {
        ValueTask<UserSession?> FindAsync(string userSessionId);
        ValueTask SaveAsync(UserSession userSession);
        ValueTask<string> NextIdAsync();

        ValueTask<IEnumerable<UserSession>> FindAllForUserAsync(string userId);
        ValueTask<IEnumerable<UserSession>> FindAllForUserFromTypingResultsAsync(string userId, DateTime fromTypingResultUtc);
    }
}
