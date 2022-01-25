using AutoFixture.Xunit2;

namespace TypingRealm.Testing;

public sealed class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoMoqDataAttribute(params object[] objects)
        : base(new AutoMoqDataAttribute(), objects) { }
}
