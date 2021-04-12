using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests
{
    public class MessageExtensionsTests
    {
        [Theory, AutoMoqData]
        public void GetMetadataOrEmpty_ShouldGetDefaultMetadata_WhenObjectIsNotWithMetadataMessage(
            object message)
        {
            var metadata = message.GetMetadataOrEmpty();

            Assert.False(metadata.RequireAcknowledgement);
            Assert.Null(metadata.MessageId);
        }

        [Theory, AutoMoqData]
        public void GetMetadataOrEmpty_ShouldGetDefaultMetadata_WhenObjectDoesNotHaveMetadata(
            ClientToServerMessageWithMetadata message)
        {
            message.Metadata = null!;

            var metadata = message.GetMetadataOrEmpty();

            Assert.False(metadata.RequireAcknowledgement);
            Assert.Null(metadata.MessageId);
        }

        [Theory, AutoMoqData]
        public void GetMetadataOrEmpty_ShouldGetMetadataFromTheMessage(
            ClientToServerMessageWithMetadata message)
        {
            var metadata = message.GetMetadataOrEmpty();

            Assert.Equal(message.Metadata, metadata);
        }
    }
}
