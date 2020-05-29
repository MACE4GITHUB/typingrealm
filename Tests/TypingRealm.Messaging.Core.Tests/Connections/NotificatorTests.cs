using System.Linq;
using AutoFixture;
using TypingRealm.Messaging.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connections
{
    public class NotificatorTests : TestsBase
    {
        [Fact]
        public void ShouldPushMessagesToBuffer_WhenNoSubscription()
        {
            var messages = Fixture.CreateMany<object>(2).ToList();

            var sut = new Notificator();
            sut.NotifyReceived(messages[0]);
            sut.NotifyReceived(messages[1]);

            Assert.Equal(2, sut.ReceivedMessagesBuffer.Count);
            Assert.Contains(messages[0], sut.ReceivedMessagesBuffer);
            Assert.Contains(messages[1], sut.ReceivedMessagesBuffer);
        }

        [Fact]
        public void ShouldPushMessagesToBufferAndNotify_WhenSubscribed()
        {
            var message = Create<object>();
            var isNotified1 = false;
            var isNotified2 = false;

            var sut = new Notificator();
            sut.Received += () => isNotified1 = true;
            sut.Received += () => isNotified2 = true;
            sut.NotifyReceived(message);

            Assert.Single(sut.ReceivedMessagesBuffer);
            Assert.Contains(message, sut.ReceivedMessagesBuffer);
            Assert.True(isNotified1);
            Assert.True(isNotified2);
        }

        [Fact]
        public void ShouldFirstPushAndThenNotify()
        {
            var message = Create<object>();
            object? result = null;

            var sut = new Notificator();
            sut.Received += () => sut.ReceivedMessagesBuffer.TryDequeue(out result);
            sut.NotifyReceived(message);

            Assert.Equal(message, result);
        }
    }
}
