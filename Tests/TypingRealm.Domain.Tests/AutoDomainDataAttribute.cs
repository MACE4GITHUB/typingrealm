using AutoFixture;
using AutoFixture.Xunit2;
using TypingRealm.Testing;

namespace TypingRealm.Domain.Tests
{
    public class AutoDomainDataAttribute : AutoDataAttribute
    {
        public AutoDomainDataAttribute() : base(() => CreateFixture()) { }

        private static IFixture CreateFixture()
        {
            return AutoMoqDataAttribute.CreateFixture()
                .Customize(new DomainCustomization());
        }
    }

    public class DomainCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new PlayerName(CreateValidPlayerName(fixture)));
        }

        private string CreateValidPlayerName(IFixture fixture)
        {
            return fixture.Create<string>().Remove(15);
        }
    }
}
