using Moq;
using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests.Json
{
    public class ConnectionExtensionsTests
    {
        [Theory, AutoMoqData]
        public void WithJson_ShouldUseJsonConnectionFactoryToCreateJsonConnection(
            IJsonConnectionFactory factory,
            JsonConnection jsonConnection,
            IConnection sut)
        {
            Mock.Get(factory).Setup(x => x.CreateJsonConnection(sut))
                .Returns(jsonConnection);

            var result = sut.WithJson(factory);

            Assert.Equal(jsonConnection, result);
        }
    }
}
