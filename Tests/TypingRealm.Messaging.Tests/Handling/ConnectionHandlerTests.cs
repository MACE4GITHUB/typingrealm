using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Handling;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Tests.SpecimenBuilders;
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

            await AssertThrowsAsync(
                () => sut.HandleAsync(connection, Cts.Token), exception);
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

            await SwallowAnyAsync(
                sut.HandleAsync(connection, Cts.Token));

            store.Verify(x => x.Add(It.IsAny<ConnectedClient>()), Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task ShouldSendDisconnectedMessage_WhenInitializerFailed(
            [Frozen]Mock<IConnectionInitializer> initializer,
            IConnection connection,
            ConnectionHandler sut)
        {
            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ThrowsAsync(Create<TestException>());

            await SwallowAnyAsync(
                sut.HandleAsync(connection, Cts.Token));

            Mock.Get(connection).Verify(x => x.SendAsync(It.IsAny<Disconnected>(), Cts.Token));
        }

        [Theory, AutoMoqData]
        public async Task ShouldNotSwallowException_WhenInitializerFailed_AndSendingDisconnectedFailed(
            [Frozen]Mock<IConnectionInitializer> initializer,
            IConnection connection,
            TestException exception,
            ConnectionHandler sut)
        {
            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ThrowsAsync(exception);

            Mock.Get(connection).Setup(x => x.SendAsync(It.IsAny<Disconnected>(), Cts.Token))
                .ThrowsAsync(Create<Exception>());

            await SwallowAnyAsync(
                sut.HandleAsync(connection, Cts.Token));

            Mock.Get(connection).Verify(x => x.SendAsync(It.IsAny<Disconnected>(), Cts.Token));
            await AssertThrowsAsync(
                () => sut.HandleAsync(connection, Cts.Token), exception);
        }

        [Theory, AutoMoqData]
        public async Task ShouldThrow_WhenStoreThrows(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IConnectedClientStore> store,
            IConnection connection,
            ConnectedClient client,
            TestException exception,
            ConnectionHandler sut)
        {
            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(client);

            store.Setup(x => x.Add(client))
                .Throws(exception);

            await AssertThrowsAsync(
                () => sut.HandleAsync(connection, Cts.Token), exception);
        }

        [Theory, AutoMoqData]
        public async Task ShouldThrow_WhenClientAlreadyConnected(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IConnectedClientStore> store,
            IConnection connection,
            ConnectedClient client,
            ConnectionHandler sut)
        {
            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(client);

            // IsClientConnected returns false.
            store.Setup(x => x.Find(client.ClientId))
                .Returns<ConnectedClient>(null);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.HandleAsync(connection, Cts.Token));
        }

        [Theory, AutoMoqData]
        public async Task ShouldAddClientToStore_WhenInitializedCorrectly(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IConnectedClientStore> store,
            ConnectedClient client,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();

            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(client);

            _ = sut.HandleAsync(connection, Cts.Token);
            await Wait();

            store.Verify(x => x.Add(client));
        }

        [Theory, AutoMoqData]
        public async Task ShouldSendPendingUpdates_AfterConnection(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IConnectedClientStore> store,
            [Frozen]Mock<IUpdateDetector> updateDetector,
            [Frozen]Mock<IUpdater> updater,
            ConnectedClient client,
            ConnectedClient anotherClient,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();

            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(client);

            var groups = new List<string>();
            updateDetector.Setup(x => x.MarkForUpdate(It.IsAny<string>()))
                .Callback<string>(group => groups.Add(group));
            updateDetector.Setup(x => x.PopMarked())
                .Returns(groups);
            store.Setup(x => x.FindInGroups(groups))
                .Returns(new[] { client, anotherClient });

            _ = sut.HandleAsync(connection, Cts.Token);
            await Wait();

            updater.Verify(x => x.SendUpdateAsync(client, Cts.Token));
            updater.Verify(x => x.SendUpdateAsync(anotherClient, Cts.Token));
        }

        [Theory, AutoMoqData]
        public async Task ShouldNotThrow_WhenSendingUpdateFails(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IConnectedClientStore> store,
            [Frozen]Mock<IUpdater> updater,
            ConnectedClient client,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();

            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(client);

            updater.Setup(x => x.SendUpdateAsync(It.IsAny<ConnectedClient>(), Cts.Token))
                .ThrowsAsync(Create<TestException>());

            var result = sut.HandleAsync(connection, Cts.Token);
            await Wait();

            store.Setup(x => x.Find(client.ClientId))
                .Returns<ConnectedClient?>(null);

            connection.Received = Create<TestMessage>();

            await result; // Completes without errors.
            Assert.True(result.IsCompletedSuccessfully);
        }


        [Theory, AutoMoqData]
        public async Task ShouldNotThrow_WhenMarkingForUpdateFails(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IConnectedClientStore> store,
            [Frozen]Mock<IUpdateDetector> updateDetector,
            ConnectedClient client,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();

            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(client);

            updateDetector.Setup(x => x.MarkForUpdate(It.IsAny<string>()))
                .Throws(Create<TestException>());

            var result = sut.HandleAsync(connection, Cts.Token);
            await Wait();

            store.Setup(x => x.Find(client.ClientId))
                .Returns<ConnectedClient?>(null);

            connection.Received = Create<TestMessage>();

            await result; // Completes without errors.
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Theory, AutoMoqData]
        public async Task ShouldReturnWithoutListening_WhenNotConnected(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IConnectedClientStore> store,
            ConnectedClient client,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();

            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(client);

            var result = sut.HandleAsync(connection, Cts.Token);
            await Wait();

            store.Setup(x => x.Find(client.ClientId))
                .Returns<ConnectedClient?>(null);

            connection.Received = Create<TestMessage>();

            await result; // Completes without errors.
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Theory, AutoMoqData]
        public async Task ShouldWaitForMessageUntilCanceled(
            [Frozen]Mock<IConnectionInitializer> initializer,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();
            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(Create<ConnectedClient>(
                    new ClientWithConnectionBuilder(connection)));

            var result = sut.HandleAsync(connection, Cts.Token);
            await Wait();
            Assert.False(result.IsCompleted);

            Cts.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => result);
        }

        [Theory, AutoMoqData]
        public async Task ShouldDispatchReceivedMessage(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IMessageDispatcher> dispatcher,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();
            var client = Create<ConnectedClient>(new ClientWithConnectionBuilder(connection));
            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(client);

            var message = Create<TestMessage>();
            var result = sut.HandleAsync(connection, Cts.Token);
            await Wait();
            Assert.False(result.IsCompleted);

            connection.Received = message;
            await Wait();

            dispatcher.Verify(x => x.DispatchAsync(client, message, Cts.Token));
        }

        [Theory, AutoMoqData]
        public async Task ShouldDispatchMultipleMessages(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IMessageDispatcher> dispatcher,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();
            var client = Create<ConnectedClient>(new ClientWithConnectionBuilder(connection));
            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(client);

            var messages = Fixture.CreateMany<TestMessage>(2).ToList();
            _ = sut.HandleAsync(connection, Cts.Token);

            connection.Received = messages[0];
            await Wait();
            dispatcher.Verify(x => x.DispatchAsync(client, messages[0], Cts.Token));

            connection.Received = messages[1];
            await Wait();
            dispatcher.Verify(x => x.DispatchAsync(client, messages[1], Cts.Token));
        }

        public enum DispatcherAction
        {
            Dispatches,
            Throws
        }
        [Theory]
        [InlineAutoMoqData(false)]
        [InlineAutoMoqData(true)]
        public async Task ShouldMarkPreviousAndNewGroupForUpdate_AfterDispatch_OrIfThrows(
            bool dispatcherThrows,
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IMessageDispatcher> dispatcher,
            [Frozen]Mock<IUpdateDetector> updateDetector,
            TestMessage message,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();
            var client = Create<ConnectedClient>(
                new ClientWithConnectionBuilder(connection));

            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(client);

            if (dispatcherThrows)
                dispatcher.Setup(x => x.DispatchAsync(client, message, Cts.Token))
                    .ThrowsAsync(Create<Exception>());

            _ = sut.HandleAsync(connection, Cts.Token);
            connection.Received = message;
            await Wait();

            updateDetector.Verify(x => x.MarkForUpdate(client.Group));
        }

        [Theory]
        [InlineAutoMoqData(false)]
        [InlineAutoMoqData(true)]
        public async Task ShouldSendUpdateToClientsInMarkedGroups_AfterDispatch_OrIfThrows(
            bool dispatcherThrows,
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IMessageDispatcher> dispatcher,
            [Frozen]Mock<IUpdateDetector> updateDetector,
            [Frozen]Mock<IConnectedClientStore> store,
            [Frozen]Mock<IUpdater> updater,
            ConnectedClient anotherClient,
            TestMessage message,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();
            var client = Create<ConnectedClient>(new ClientWithConnectionBuilder(connection));
            initializer.Setup(x => x.ConnectAsync(It.IsAny<IConnection>(), Cts.Token))
                .ReturnsAsync(client);

            if (dispatcherThrows)
                dispatcher.Setup(x => x.DispatchAsync(client, message, Cts.Token))
                    .ThrowsAsync(Create<Exception>());

            var anotherGroup = Create<string>();
            var popGroups = new List<string> { anotherGroup };
            store.Setup(x => x.FindInGroups(It.Is<IEnumerable<string>>(
                y => y.Count() == 2
                && y.Contains(anotherGroup)
                && y.Contains(client.Group))))
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
        public async Task ShouldThrow_WhenDispatcherThrows(
            [Frozen]Mock<IMessageDispatcher> dispatcher,
            TestException exception,
            ConnectionHandler sut)
        {
            dispatcher.Setup(x => x.DispatchAsync(It.IsAny<ConnectedClient>(), It.IsAny<object>(), Cts.Token))
                .ThrowsAsync(exception);

            await AssertThrowsAsync(
                () => sut.HandleAsync(Create<IConnection>(), Cts.Token),
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
                () => sut.HandleAsync(Create<IConnection>(), Cts.Token),
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
            var client = Create<ConnectedClient>(
                new ClientWithIdBuilder(clientId),
                new ClientWithConnectionBuilder(connection));

            dispatcher.Setup(x => x.DispatchAsync(client, It.IsAny<object>(), Cts.Token))
                .ThrowsAsync(Create<Exception>());

            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(client);

            await SwallowAnyAsync(sut.HandleAsync(connection, Cts.Token));
            store.Verify(x => x.Remove(clientId));
        }

        [Theory, AutoMoqData]
        public async Task ShouldSendDisconnectedMessage_WhenDispatcherThrows(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IMessageDispatcher> dispatcher,
            IConnection connection,
            ConnectionHandler sut)
        {
            dispatcher.Setup(x => x.DispatchAsync(It.IsAny<ConnectedClient>(), It.IsAny<object>(), Cts.Token))
                .ThrowsAsync(Create<Exception>());

            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(Create<ConnectedClient>(
                    new ClientWithConnectionBuilder(connection)));

            await SwallowAnyAsync(sut.HandleAsync(connection, Cts.Token));

            Mock.Get(connection).Verify(x => x.SendAsync(It.IsAny<Disconnected>(), Cts.Token));
        }

        [Theory, AutoMoqData]
        public async Task ShouldHandleFullIntegrationScenario(
            [Frozen]Mock<IConnectionInitializer> initializer,
            [Frozen]Mock<IConnectedClientStore> store,
            [Frozen]Mock<IMessageDispatcher> dispatcher,
            [Frozen]Mock<IUpdateDetector> updateDetector,
            [Frozen]Mock<IUpdater> updater,
            ConnectionHandler sut)
        {
            var connection = new TestConnection();
            var client = Create<ConnectedClient>(
                new ClientWithConnectionBuilder(connection),
                new ClientWithUpdateDetectorBuilder(updateDetector.Object));

            initializer.Setup(x => x.ConnectAsync(connection, Cts.Token))
                .ReturnsAsync(client);

            var groups = Fixture.CreateMany<string>();
            updateDetector.Setup(x => x.PopMarked())
                .Returns(groups);
            store.Setup(x => x.FindInGroups(groups))
                .Returns(new[] { client });

            _ = sut.HandleAsync(connection, Cts.Token);
            await Wait();
            dispatcher.Verify(x => x.DispatchAsync(client, It.IsAny<object>(), Cts.Token), Times.Never);

            var message = Create<object>();
            connection.Received = message;
            await Wait();
            dispatcher.Verify(x => x.DispatchAsync(client, message, Cts.Token));

            message = Create<TestMessage>();
            var newGroup = Create<string>();
            dispatcher.Setup(x => x.DispatchAsync(client, message, Cts.Token))
                .Callback<ConnectedClient, object, CancellationToken>(
                    (client, message, token) =>
                    {
                        client.Group = newGroup;
                    });

            updateDetector.Verify(x => x.MarkForUpdate(client.Group), Times.Exactly(2));
            updater.Verify(x => x.SendUpdateAsync(client, Cts.Token), Times.Exactly(2));

            var currentGroup = client.Group;
            connection.Received = message;
            await Wait();

            // After the group has changed, previous group should be marked by Setter.
            updateDetector.Verify(x => x.MarkForUpdate(currentGroup), Times.Exactly(3));

            // One time from ConnectedClient.Group setter, another one from ConnectionHandler.
            // Consider changing this logic.
            updateDetector.Verify(x => x.MarkForUpdate(newGroup), Times.Exactly(2));

            updater.Verify(x => x.SendUpdateAsync(client, Cts.Token), Times.Exactly(3));
        }
    }
}
