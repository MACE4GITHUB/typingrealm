using AutoFixture;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Messaging.Tests.SpecimenBuilders
{
    public class SingleGroupCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() =>
            {
                return new ConnectedClient(
                    fixture.Create<string>(),
                    fixture.Create<IConnection>(),
                    fixture.Create<IUpdateDetector>(),
                    fixture.Create<string>());
            });
        }
    }
}
