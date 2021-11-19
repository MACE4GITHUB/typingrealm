using System.Threading.Tasks;
using TypingRealm.Typing.Framework;

namespace TypingRealm.Typing
{
    public sealed class InMemoryUserSessionRepository : IUserSessionRepository
    {
        private readonly InMemoryRepository<UserSession.State> _store
            = new InMemoryRepository<UserSession.State>();

        public async ValueTask<UserSession?> FindAsync(string userSessionId)
        {
            var state = await _store.FindAsync(userSessionId)
                .ConfigureAwait(false);

            if (state == null)
                return null;

            return UserSession.FromState(state);
        }

        public ValueTask<string> NextIdAsync()
        {
            return _store.NextIdAsync();
        }

        public ValueTask SaveAsync(UserSession userSession)
        {
            var state = userSession.GetState();

            return _store.SaveAsync(state);
        }
    }
}
