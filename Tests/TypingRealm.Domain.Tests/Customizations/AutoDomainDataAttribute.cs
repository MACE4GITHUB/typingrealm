using AutoFixture;
using AutoFixture.Xunit2;
using TypingRealm.Testing;

namespace TypingRealm.Domain.Tests.Customizations
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

    public class RoamingAutoDomainDataAttribute : AutoDataAttribute
    {
        public RoamingAutoDomainDataAttribute() : base(() => CreateFixture()) { }

        private static IFixture CreateFixture()
        {
            return AutoMoqDataAttribute.CreateFixture()
                .Customize(new RoamingDomainCustomization());
        }
    }

    public class InBattleAutoDomainDataAttribute : AutoDataAttribute
    {
        public InBattleAutoDomainDataAttribute() : base(() => CreateFixture()) { }

        private static IFixture CreateFixture()
        {
            return AutoMoqDataAttribute.CreateFixture()
                .Customize(new InBattleDomainCustomization());
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

    public class RoamingDomainCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new PlayerName(CreateValidPlayerName(fixture)));
            fixture.Customizations.Add(new PlayerNotInBattle());
        }

        private string CreateValidPlayerName(IFixture fixture)
        {
            return fixture.Create<string>().Remove(15);
        }
    }


    public class InBattleDomainCustomization : ICustomization
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
