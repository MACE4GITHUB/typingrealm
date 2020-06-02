using System;
using System.Threading.Tasks;
using Moq;
using TypingRealm.Messaging;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Domain.Tests
{
    public class ConnectionInitializerTests : TestsBase
    {
        [Theory, AutoMoqData]
        public async Task ShouldThrow_WhenFirstMessageIsNotJoin(
            IConnection connection,
            ConnectionInitializer sut)
        {
            Mock.Get(connection).Setup(x => x.ReceiveAsync(Cts.Token))
                .ReturnsAsync(Create<TestMessage>());

            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await sut.ConnectAsync(connection, Cts.Token));
        }
    }
}
