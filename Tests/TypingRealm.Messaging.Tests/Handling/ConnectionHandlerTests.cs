using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Handling;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Handling
{
    public class TestConnection : IConnection
    {
        public object? Received { get; set; }
        public bool Sent { get; set; }

        public async ValueTask<object> ReceiveAsync(CancellationToken cancellationToken)
        {
            while (Received == null)
            {
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();
            }

            var received = Received;
            Received = null;

            return received;
        }

        public async ValueTask SendAsync(object message, CancellationToken cancellationToken)
        {
            while (!Sent)
            {
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }

    public class ConnectionHandlerTests : TestsBase
    {
        [Theory, AutoMoqData]
        public async Task ShouldThrow_WhenInitializerThrows(
            [Frozen]Mock<IConnectionInitializer> initializer,
            IConnection connection,
            TestException exception,
            ConnectionHandler sut)
        {
            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ThrowsAsync(exception);

            await AssertThrowsAsync(sut.HandleAsync(connection, Cts.Token), exception);
        }

        [Theory, AutoMoqData]
        public async Task ShouldNotAddClient_WhenInitializerThrows(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IConnectedClientStore> store,
            IConnection connection,
            ConnectionHandler sut)
        {
            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ThrowsAsync(Create<TestException>());

            try { await sut.HandleAsync(connection, Cts.Token); } catch { }

            store.Verify(x => x.Add(It.IsAny<ConnectedClient>()), Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task ShouldThrow_WhenStoreThrows(
            [Frozen]Mock<IConnectedClientStore> store,
            TestException exception,
            ConnectionHandler sut)
        {
            store.Setup(x => x.Add(It.IsAny<ConnectedClient>()))
                .Throws(exception);

            await AssertThrowsAsync(sut.HandleAsync(Create<IConnection>(), Cts.Token), exception);
        }

        [Theory, AutoMoqData]
        public async Task ShouldAddClientToStore_WhenInitializedCorrectly(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IConnectedClientStore> store,
            IConnection connection,
            ConnectedClient client,
            ConnectionHandler sut)
        {
            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(client);

            store.Setup(x => x.Find(client.ClientId))
                .Returns<ConnectedClient>(null);

            await sut.HandleAsync(connection, Cts.Token);

            store.Verify(x => x.Add(client), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task ShouldReturnWithoutListening_WhenNotConnected(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IConnectedClientStore> store,
            Mock<IConnection> connection,
            ConnectedClient client,
            ConnectionHandler sut)
        {
            initializer.Setup(x => x.ConnectAsync(connection.Object, Cts.Token))
                .ReturnsAsync(client);

            store.Setup(x => x.Find(client.ClientId))
                .Returns<ConnectedClient>(null);

            await sut.HandleAsync(connection.Object, Cts.Token);

            connection.Verify(x => x.ReceiveAsync(Cts.Token), Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task ShouldWaitForMessageUntilCanceled(
            [Frozen]Mock<IConnectionInitializer> initializer,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();
            initializer.Setup(x => x.ConnectAsync(It.IsAny<IConnection>(), Cts.Token))
                .ReturnsAsync(new ConnectedClient(Create<string>(), connection, Create<string>(), Create<IUpdateDetector>()));

            using var cts = new CancellationTokenSource();
            var result = sut.HandleAsync(connection, cts.Token);
            await Wait();
            Assert.False(result.IsCompleted);

            cts.Cancel();
            await AssertThrowsAsync<OperationCanceledException>(result);
        }

        [Theory, AutoMoqData]
        public async Task ShouldDispatchReceivedMessage(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IMessageDispatcher> dispatcher,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();
            var client = new ConnectedClient(Create<string>(), connection, Create<string>(), Create<IUpdateDetector>());
            initializer.Setup(x => x.ConnectAsync(It.IsAny<IConnection>(), Cts.Token))
                .ReturnsAsync(client);

            var message = Create<TestMessage>();
            var result = sut.HandleAsync(connection, Cts.Token);
            await Wait();
            Assert.False(result.IsCompleted);

            connection.Received = message;
            await Wait();

            dispatcher.Verify(x => x.DispatchAsync(client, message, Cts.Token));

            Cts.Cancel();
        }

        [Theory, InlineAutoMoqData(false), InlineAutoMoqData(true)]
        public async Task ShouldMarkPreviousAndNewGroupsForUpdate_WhenDispatchedOrDispatcherThrows(
            bool dispatcherThrows,
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IUpdateDetector> updateDetector,
            [Frozen]Mock<IMessageDispatcher> dispatcher,
            string initialGroup,
            string updatedGroup,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();
            var client = CreateClient(connection, initialGroup);
            initializer.Setup(x => x.ConnectAsync(It.IsAny<IConnection>(), Cts.Token))
                .ReturnsAsync(client);

            var message = Create<TestMessage>();

            if (dispatcherThrows)
            {
                dispatcher.Setup(x => x.DispatchAsync(client, message, It.IsAny<CancellationToken>()))
                    .Callback(() => client.Group = updatedGroup)
                    .Returns(new ValueTask(Task.FromException(Create<TestException>())));
            }
            else
            {
                dispatcher.Setup(x => x.DispatchAsync(client, message, It.IsAny<CancellationToken>()))
                    .Callback(() => client.Group = updatedGroup);
            }

            _ = sut.HandleAsync(connection, Cts.Token);
            connection.Received = message;
            await Wait();

            updateDetector.Verify(x => x.MarkForUpdate(initialGroup));
            updateDetector.Verify(x => x.MarkForUpdate(updatedGroup));
            Cts.Cancel();
        }

        [Theory, AutoMoqData]
        public async Task ShouldThrow_WhenDispatcherThrows(
            [Frozen]Mock<IMessageDispatcher> dispatcher,
            TestException exception,
            ConnectionHandler sut)
        {
            dispatcher.Setup(x => x.DispatchAsync(It.IsAny<ConnectedClient>(), It.IsAny<object>(), Cts.Token))
                .Returns(new ValueTask(Task.FromException(exception)));

            await AssertThrowsAsync(
                sut.HandleAsync(Create<IConnection>(), Cts.Token),
                exception);
        }

        [Theory, AutoMoqData]
        public async Task ShouldThrowDispatcherException_WhenDispatcherThrows_AndUpdateDetectorThrows(
            [Frozen]Mock<IMessageDispatcher> dispatcher,
            [Frozen]Mock<IUpdateDetector> updateDetector,
            TestException exception,
            ConnectionHandler sut)
        {
            dispatcher.Setup(x => x.DispatchAsync(It.IsAny<ConnectedClient>(), It.IsAny<object>(), Cts.Token))
                .Returns(new ValueTask(Task.FromException(exception)));

            updateDetector.Setup(x => x.MarkForUpdate(It.IsAny<string>()))
                .Throws(Create<Exception>());

            await AssertThrowsAsync(
                sut.HandleAsync(Create<IConnection>(), Cts.Token),
                exception);
        }

        [Theory, AutoMoqData]
        public async Task ShouldRemoveClientFromStore_WhenDispatcherThrows(
            [Frozen]Mock<IMessageDispatcher> dispatcher,
            [Frozen]Mock<IConnectedClientStore> store,
            [Frozen]Mock<IConnectionInitializer> initializer,
            string clientId,
            IConnection connection,
            ConnectionHandler sut)
        {
            dispatcher.Setup(x => x.DispatchAsync(It.IsAny<ConnectedClient>(), It.IsAny<object>(), Cts.Token))
                .Returns(new ValueTask(Task.FromException(Create<Exception>())));

            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(new ConnectedClient(clientId, connection, Create<string>(), Create<IUpdateDetector>()));

            await AssertThrowsAsync<Exception>(sut.HandleAsync(connection, Cts.Token));

            store.Verify(x => x.Remove(clientId));
        }

        [Theory, InlineAutoMoqData(false), InlineAutoMoqData(true)]
        public async Task ShouldSendUpdateToRelevantClients_WhenDispatchedOrDispatcherThrows(
            bool dispatcherThrows,
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IUpdateDetector> updateDetector,
            [Frozen]Mock<IMessageDispatcher> dispatcher,
            [Frozen]Mock<IConnectedClientStore> store,
            [Frozen]Mock<IUpdater> updater,
            string initialGroup,
            string updatedGroup,
            Mock<IConnection> anotherConnection,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();
            var client = CreateClient(connection, initialGroup);
            var anotherClient = CreateClient(anotherConnection.Object, initialGroup);
            initializer.Setup(x => x.ConnectAsync(It.IsAny<IConnection>(), Cts.Token))
                .ReturnsAsync(client);

            var message = Create<TestMessage>();

            if (dispatcherThrows)
            {
                dispatcher.Setup(x => x.DispatchAsync(client, message, Cts.Token))
                    .Callback(() => client.Group = updatedGroup)
                    .Returns(new ValueTask(Task.FromException(Create<TestException>())));
            }
            else
            {
                dispatcher.Setup(x => x.DispatchAsync(client, message, Cts.Token))
                    .Callback(() => client.Group = updatedGroup);
            }

            var anotherGroup = Create<string>();
            var popGroups = new List<string> { anotherGroup };
            store.Setup(x => x.FindInGroups(It.Is<IEnumerable<string>>(
                y => y.Count() == 3
                && y.Contains(anotherGroup)
                && y.Contains(initialGroup)
                && y.Contains(updatedGroup))))
                .Returns(new[] { client, anotherClient });

            updateDetector.Setup(x => x.MarkForUpdate(It.IsAny<string>()))
                .Callback<string>(group => popGroups.Add(group));
            updateDetector.Setup(x => x.PopMarked())
                .Returns(popGroups);

            _ = sut.HandleAsync(connection, Cts.Token);
            connection.Received = message;
            await Wait();

            updater.Verify(x => x.SendUpdateAsync(client, Cts.Token));
            updater.Verify(x => x.SendUpdateAsync(anotherClient, Cts.Token));
        }

        [Theory, AutoMoqData]
        public async Task ShouldSendDisconnectedMessage_WhenDispatcherThrows(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IMessageDispatcher> dispatcher,
            Mock<IConnection> connection,
            ConnectionHandler sut)
        {
            dispatcher.Setup(x => x.DispatchAsync(It.IsAny<ConnectedClient>(), It.IsAny<object>(), Cts.Token))
                .Returns(new ValueTask(Task.FromException(Create<TestException>())));


            initializer.Setup(x => x.ConnectAsync(connection.Object, Cts.Token))
                .ReturnsAsync(CreateClient(connection.Object));

            await AssertThrowsAsync<TestException>(sut.HandleAsync(connection.Object, Cts.Token));

            connection.Verify(x => x.SendAsync(It.IsAny<Disconnected>(), Cts.Token));
        }

        [Theory, AutoMoqData]
        public async Task ShouldThrowException_WhenUpdateDetectorThrows(
            [Frozen]Mock<IUpdateDetector> updateDetector,
            ConnectionHandler sut)
        {
            updateDetector.Setup(x => x.MarkForUpdate(It.IsAny<string>()))
                .Throws<TestException>();

            await AssertThrowsAsync<TestException>(sut.HandleAsync(Create<IConnection>(), Cts.Token));
        }

        [Theory, AutoMoqData]
        public async Task ShouldThrowException_WhenUpdaterThrows(
            [Frozen]Mock<IUpdater> updater,
            ConnectionHandler sut)
        {
            updater.Setup(x => x.SendUpdateAsync(It.IsAny<ConnectedClient>(), Cts.Token))
                .Returns(new ValueTask(Task.FromException(Create<TestException>())));

            await AssertThrowsAsync<TestException>(sut.HandleAsync(Create<IConnection>(), Cts.Token));
        }

        private ConnectedClient CreateClient(IConnection connection, string? initialGroup = null)
        {
            return new ConnectedClient(Create<string>(), connection, initialGroup ?? Create<string>(), Create<IUpdateDetector>());
        }
    }
}
