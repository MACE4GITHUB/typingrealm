using AutoFixture;
using AutoFixture.Xunit2;
using TypingRealm.Testing;

namespace TypingRealm.Messaging.Tests.SpecimenBuilders
{
    public class SingleGroupDataAttribute : AutoDataAttribute
    {
        public SingleGroupDataAttribute() : base(() => CreateFixture()) { }

        private static IFixture CreateFixture()
        {
            return AutoMoqDataAttribute.CreateFixture()
                .Customize(new SingleGroupCustomization());
        }
    }
}
