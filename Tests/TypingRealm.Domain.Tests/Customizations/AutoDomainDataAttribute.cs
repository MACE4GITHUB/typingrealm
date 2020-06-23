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

    public class DomainCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
        }
    }
}
