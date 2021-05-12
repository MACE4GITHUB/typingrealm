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
            Assert.Equal(AcknowledgementType.Handled, sut.AcknowledgementType);
            Assert.Null(sut.ResponseMessageTypeId);
        }

        [Fact]
        public void ShouldBeSerializable()
        {
            AssertSerializable<ClientToServerMessageMetadata>();
        }
    }
}
