using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests
{
    public class ClientToServerMessageWithMetadataTests : TestsBase
    {
        [Theory, AutoMoqData]
        public void ShouldBeSerializable(
            string message,
            ClientToServerMessageMetadata metadata)
        {
            AssertSerializable<ClientToServerMessageWithMetadata>();

            var sut = new ClientToServerMessageWithMetadata(message, metadata);
            Assert.Equal(message, sut.Message);
            Assert.Equal(metadata, sut.Metadata);
        }
    }
}
