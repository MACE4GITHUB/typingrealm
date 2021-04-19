using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests
{
    public class ClientToServerMessageMetadataTests : TestsBase
    {
        [Fact]
        public void ShouldCreateEmpty()
        {
            var sut = ClientToServerMessageMetadata.CreateEmpty();

            Assert.Null(sut.MessageId);
            Assert.False(sut.RequireAcknowledgement);
            Assert.Null(sut.ResponseMessageTypeId);
        }

        [Theory, AutoMoqData]
        public void ShouldEnableAcknowledgement(string messageId)
        {
            var sut = ClientToServerMessageMetadata.CreateEmpty();

            sut.EnableAcknowledgement(messageId);

            Assert.Equal(messageId, sut.MessageId);
            Assert.True(sut.RequireAcknowledgement);
            Assert.Null(sut.ResponseMessageTypeId);
        }

        [Fact]
        public void ShouldBeSerializable()
        {
            AssertSerializable<ClientToServerMessageMetadata>();
        }
    }
}
