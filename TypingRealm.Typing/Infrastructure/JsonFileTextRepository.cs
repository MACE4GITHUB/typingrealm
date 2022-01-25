using TypingRealm.Typing.Framework;

namespace TypingRealm.Typing.Infrastructure;

public sealed class JsonFileTextRepository : StateBasedRepository<Text, Text.State>, ITextRepository
{
    public JsonFileTextRepository()
        : base(new JsonFileRepository<Text.State>("texts.json")) { }

    protected override Text CreateFromState(Text.State state) => Text.FromState(state);
    protected override Text.State GetFromEntity(Text entity) => entity.GetState();
}
