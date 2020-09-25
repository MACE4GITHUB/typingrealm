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
            [Frozen]IProtobufFieldNumberCache cache,
            [Frozen]IProtobuf protobuf,
            Stream stream,
            ProtobufConnectionFactory sut)
        {
            var connection = sut.CreateProtobufConnection(stream);

            Assert.Equal(cache, GetPrivateField(connection, "_fieldNumberCache"));
            Assert.Equal(stream, GetPrivateField(connection, "_stream"));
            Assert.Equal(protobuf, GetPrivateField(connection, "_protobuf"));
        }
    }
}
