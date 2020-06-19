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
            // Previously we set PlayerName constraints here.
            // No specific domain customizations needed for now.
        }
    }

    public class RoamingDomainCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new PlayerNotInBattle());
        }
    }


    public class InBattleDomainCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
        }
    }
}
