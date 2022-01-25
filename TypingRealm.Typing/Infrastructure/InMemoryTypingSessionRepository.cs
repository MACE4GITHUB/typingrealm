using TypingRealm.Typing.Framework;

namespace TypingRealm.Typing.Infrastructure;

public sealed class InMemoryTypingSessionRepository : StateBasedRepository<TypingSession, TypingSession.State>, ITypingSessionRepository
{
    public InMemoryTypingSessionRepository() : base(new InMemoryRepository<TypingSession.State>()) { }

    protected override TypingSession CreateFromState(TypingSession.State state) => TypingSession.FromState(state);
    protected override TypingSession.State GetFromEntity(TypingSession entity) => entity.GetState();
}
