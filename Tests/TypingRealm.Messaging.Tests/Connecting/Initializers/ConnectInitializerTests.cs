using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Connecting.Initializers;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connecting.Initializers
{
    public class ConnectInitializerTests : MessagingTestsBase
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
            VerifyMarkedForUpdate(Mock.Get(updateDetector), "new");
        }

        [Theory, AutoMoqData]
        public async Task ShouldCallConnectHook(
            [Frozen]Mock<IConnectHook> connectHook,
            ConnectInitializer sut)
        {
            await sut.ConnectAsync(_validConnection, Cts.Token);

            connectHook.Verify(x => x.HandleAsync(_connectMessage, Cts.Token));
        }

        [Theory, AutoMoqData]
        public void ShouldRegisterConnectHooksInOrder(
            IConnectHook ch1, EmptyConnectHook ch2)
        {
            var provider = new ServiceCollection()
                .AddTransient(sp => ch1)
                .AddTransient<IConnectHook>(sp => ch2)
                .BuildServiceProvider();

            Assert.Equal(2, provider.GetServices<IConnectHook>().Count());
            Assert.Equal(ch1, provider.GetServices<IConnectHook>().First());
            Assert.Equal(ch2, provider.GetServices<IConnectHook>().ToList()[1]);
        }

        [Theory, AutoMoqData]
        public async Task ShouldCallManyConnectHooksInOrder(
            [Frozen]IEnumerable<IConnectHook> connectHooks,
            ConnectInitializer sut)
        {
            var hooks = connectHooks.ToList();
            var count = 0;

            Mock.Get(hooks[0]).Setup(x => x.HandleAsync(It.IsAny<Connect>(), Cts.Token))
                .Callback(() =>
                {
                    if (count == 0)
                        count++;
                });

            Mock.Get(hooks[1]).Setup(x => x.HandleAsync(It.IsAny<Connect>(), Cts.Token))
                .Callback(() =>
                {
                    if (count == 1)
                        count++;
                });

            Mock.Get(hooks[2]).Setup(x => x.HandleAsync(It.IsAny<Connect>(), Cts.Token))
                .Callback(() =>
                {
                    if (count == 2)
                        count++;
                });

            await sut.ConnectAsync(_validConnection, Cts.Token);

            foreach (var connectHook in connectHooks)
            {
                Mock.Get(connectHook).Verify(x => x.HandleAsync(_connectMessage, Cts.Token));
            }

            Assert.Equal(3, count);
        }

        [Theory, AutoMoqData]
        public async Task ShouldThrow_WhenConnectHookThrows(
            Exception exception,
            [Frozen]Mock<IConnectHook> connectHook,
            ConnectInitializer sut)
        {
            connectHook.Setup(x => x.HandleAsync(_connectMessage, Cts.Token))
                .ThrowsAsync(exception);

            var thrown = await Assert.ThrowsAsync<Exception>(
                async () => await sut.ConnectAsync(_validConnection, Cts.Token));

            Assert.Equal(exception, thrown);
        }
    }
}
