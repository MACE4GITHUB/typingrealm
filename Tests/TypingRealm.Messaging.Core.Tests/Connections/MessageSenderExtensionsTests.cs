using TypingRealm.Messaging.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connections
{
    public class MessageSenderExtensionsTests : TestsBase
    {
        [Theory, AutoMoqData]
        public void WithNotificator_ShouldSetNotificator(
            Notificator notificator,
            IMessageSender sut)
        {
            var connection = sut.WithNotificator(notificator);

            Assert.Equal(notificator, GetPrivateField(connection, "_notificator"));
        }

        [Theory, AutoMoqData]
        public void WithNotificator_ShouldSetMessageSender(IMessageSender sut)
        {
            var connection = sut.WithNotificator(Create<Notificator>());

            Assert.Equal(sut, GetPrivateField(connection, "_messageSender"));
        }
    }
}
