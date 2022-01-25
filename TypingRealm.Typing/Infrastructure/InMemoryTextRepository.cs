using TypingRealm.Typing.Framework;

namespace TypingRealm.Typing.Infrastructure;

public sealed class InMemoryTextRepository : StateBasedRepository<Text, Text.State>, ITextRepository
{
    public InMemoryTextRepository() : base(new InMemoryRepository<Text.State>()) { }

    protected override Text CreateFromState(Text.State state) => Text.FromState(state);
    protected override Text.State GetFromEntity(Text entity) => entity.GetState();
}
