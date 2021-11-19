using TypingRealm.Typing.Framework;

namespace TypingRealm.Typing.Infrastructure
{
    public sealed class JsonFileTypingSessionRepository : StateBasedRepository<TypingSession, TypingSession.State>, ITypingSessionRepository
    {
        public JsonFileTypingSessionRepository()
            : base(new JsonFileRepository<TypingSession.State>("typing-sessions.json")) { }

        protected override TypingSession CreateFromState(TypingSession.State state) => TypingSession.FromState(state);
        protected override TypingSession.State GetFromEntity(TypingSession entity) => entity.GetState();
    }


}
