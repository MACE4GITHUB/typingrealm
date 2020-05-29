using AutoFixture.Xunit2;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests.Json
{
    public class JsonConnectionFactoryTests : TestsBase
    {
        [Theory, AutoMoqData]
        public void ShouldSetMessageTypeCache(
            [Frozen]IMessageTypeCache cache,
            IConnection connection,
            JsonConnectionFactory sut)
        {
            var result = sut.CreateJsonConnection(connection);
            Assert.Equal(cache, GetPrivateField(result, "_messageTypes"));
        }

        [Theory, AutoMoqData]
        public void ShouldSetInnerConnection(
            IConnection connection,
            JsonConnectionFactory sut)
        {
            var result = sut.CreateJsonConnection(connection);
            Assert.Equal(connection, GetPrivateField(result, "_connection"));
        }
    }
}
