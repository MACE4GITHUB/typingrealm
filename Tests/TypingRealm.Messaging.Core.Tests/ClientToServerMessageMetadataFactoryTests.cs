using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests
{
    public class ClientToServerMessageMetadataFactoryTests
    {
        [Theory, AutoMoqData]
        public void ShouldHaveFalseAcknowledgementByDefault(
            object message,
            ClientToServerMessageMetadataFactory sut)
        {
            var result = sut.CreateFor(message);
            Assert.False(result.RequireReceivedAcknowledgement);
        }

        [Theory, AutoMoqData]
        public void ShouldGetMessageIdFromFactory(
            [Frozen]Mock<IMessageIdFactory> messageIdFactory,
            string messageId,
            object message,
            ClientToServerMessageMetadataFactory sut)
        {
            messageIdFactory.Setup(x => x.CreateMessageId())
                .Returns(messageId);

            var result = sut.CreateFor(message);

            Assert.Equal(messageId, result.MessageId);
        }
    }
}
