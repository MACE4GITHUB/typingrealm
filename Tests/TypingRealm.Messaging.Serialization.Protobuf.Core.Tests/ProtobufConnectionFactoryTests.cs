using System.IO;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Protobuf.Tests
{
    public class ProtobufConnectionFactoryTests : TestsBase
    {
        [Theory, AutoMoqData]
        public async Task ShouldCreateWithStreamAndCache(
            [Frozen]IMessageTypeCache cache,
            Stream stream,
            ProtobufConnectionFactory sut)
        {
            var connection = sut.CreateProtobufConnection(stream);

            Assert.Equal(cache, GetPrivateField(connection, "_messageTypes"));
            Assert.Equal(stream, GetPrivateField(connection, "_stream"));
        }
    }
}
