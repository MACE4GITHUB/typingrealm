using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Connecting.Initializers;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connecting.Initializers
{
    public class ConnectInitializerTests : TestsBase
    {
        private readonly Connect _connectMessage;
        private readonly IConnection _validConnection;

        public ConnectInitializerTests()
        {
            _validConnection = Create<IConnection>();
            _connectMessage = Create<Connect>();
            Mock.Get(_validConnection).Setup(x => x.ReceiveAsync(Cts.Token))
                .ReturnsAsync(_connectMessage);
        }

        [Theory, AutoMoqData]
        public async Task ShouldThrow_WhenFirstMessageIsNotConnect(
            IConnection connection,
            ConnectInitializer sut)
        {
            Mock.Get(connection).Setup(x => x.ReceiveAsync(Cts.Token))
                .ReturnsAsync(Create<TestMessage>());

            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await sut.ConnectAsync(connection, Cts.Token));
        }

        [Theory, AutoMoqData]
        public async Task ShouldSendDisconnectedMessage_WhenFirstMessageIsNotConnect(
            IConnection connection,
            ConnectInitializer sut)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await sut.ConnectAsync(connection, Cts.Token));

            Mock.Get(connection).Verify(x => x.SendAsync(It.IsAny<Disconnected>(), Cts.Token));
        }

        [Theory, AutoMoqData]
        public async Task ShouldSetClientId(ConnectInitializer sut)
        {
            var client = await sut.ConnectAsync(_validConnection, Cts.Token);

            Assert.Equal(_connectMessage.ClientId, client.ClientId);
        }

        [Theory, AutoMoqData]
        public async Task ShouldSetGroup(ConnectInitializer sut)
        {
            var client = await sut.ConnectAsync(_validConnection, Cts.Token);

            Assert.Equal(_connectMessage.Group, client.Group);
        }

        [Theory, AutoMoqData]
        public async Task ShouldSetConnection(ConnectInitializer sut)
        {
            var client = await sut.ConnectAsync(_validConnection, Cts.Token);

            Assert.Equal(_validConnection, client.Connection);
        }

        [Theory, AutoMoqData]
        public async Task ShouldSetUpdateDetector(
            [Frozen]IUpdateDetector updateDetector,
            ConnectInitializer sut)
        {
            var client = await sut.ConnectAsync(_validConnection, Cts.Token);

            Assert.Equal(updateDetector, GetPrivateField(client, "_updateDetector"));

            client.Group = "new";
            Mock.Get(updateDetector).Verify(x => x.MarkForUpdate("new"));
        }
    }
}
