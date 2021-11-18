using System.Threading.Tasks;
using TypingRealm.Typing.Framework;

namespace TypingRealm.Typing
{
    public sealed class InMemoryTypingSessionRepository : ITypingSessionRepository
    {
        private readonly InMemoryRepository<TypingSession.State> _store
            = new InMemoryRepository<TypingSession.State>();

        public async ValueTask<TypingSession?> FindAsync(string typingSessionId)
        {
            var state = await _store.FindAsync(typingSessionId)
                .ConfigureAwait(false);

            if (state == null)
                return null;

            return TypingSession.FromState(state);
        }

        public ValueTask<string> NextIdAsync()
        {
            return _store.NextIdAsync();
        }

        public ValueTask SaveAsync(TypingSession typingSession)
        {
            var state = typingSession.GetState();

            return _store.SaveAsync(state);
        }
    }
}
