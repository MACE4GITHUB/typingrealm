using System.Threading.Tasks;
using TypingRealm.Typing.Framework;

namespace TypingRealm.Typing.Infrastructure
{
    public sealed class InMemoryTextRepository : ITextRepository
    {
        private readonly InMemoryRepository<Text.State> _store
            = new InMemoryRepository<Text.State>();

        public async ValueTask<Text?> FindAsync(string textId)
        {
            var state = await _store.FindAsync(textId)
                .ConfigureAwait(false);

            if (state == null)
                return null;

            return Text.FromState(state);
        }

        public ValueTask<string> NextIdAsync()
        {
            return _store.NextIdAsync();
        }

        public ValueTask SaveAsync(Text text)
        {
            var state = text.GetState();

            return _store.SaveAsync(state);
        }
    }
}
