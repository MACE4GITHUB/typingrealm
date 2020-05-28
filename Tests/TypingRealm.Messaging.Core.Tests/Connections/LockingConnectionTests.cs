using System;
using System.Threading.Tasks;
using Moq;
using TypingRealm.Messaging.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connections
{
    public class LockingConnectionTests : TestsBase
    {
        private readonly TestLock _sendLock;
        private readonly TestLock _receiveLock;
        private readonly Mock<IConnection> _connection;
        private readonly LockingConnection _sut;
        private readonly object _message;

        public LockingConnectionTests()
        {
            _connection = Create<Mock<IConnection>>();
            _sendLock = new TestLock();
            _receiveLock = new TestLock();
            _sut = new LockingConnection(_connection.Object, _sendLock, _receiveLock);
            _message = Create<object>();

            _connection.Setup(x => x.ReceiveAsync(Cts.Token))
                .ReturnsAsync(_message);
        }

        [Theory, AutoMoqData]
        public void ShouldThrowWhenSendAndReceiveLocksIsTheSameLock(
            ILock @lock, IConnection connection)
        {
            Assert.Throws<ArgumentException>(
                () => new LockingConnection(connection, @lock, @lock));
        }

        [Fact]
        public async Task ShouldLockOnSend()
        {
            var waiting = _sut.SendAsync(_message, Cts.Token);
            await Wait();

            Assert.False(waiting.IsCompleted);
            Assert.Equal(TestLock.State.Started, _sendLock.WaitState);
            Assert.Equal(TestLock.State.None, _sendLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.None, _receiveLock.ReleaseState);
            _connection.Verify(x => x.SendAsync(_message, Cts.Token), Times.Never);

            _sendLock.WaitComplete = true;
            await Wait();

            Assert.False(waiting.IsCompleted);
            Assert.Equal(TestLock.State.Completed, _sendLock.WaitState);
            Assert.Equal(TestLock.State.Started, _sendLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.None, _receiveLock.ReleaseState);
            _connection.Verify(x => x.SendAsync(_message, Cts.Token));

            _sendLock.ReleaseComplete = true;
            await waiting;

            Assert.True(waiting.IsCompletedSuccessfully);
            Assert.Equal(TestLock.State.Completed, _sendLock.WaitState);
            Assert.Equal(TestLock.State.Completed, _sendLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.None, _receiveLock.ReleaseState);
        }

        [Fact]
        public async Task ShouldLockOnReceive()
        {
            var waiting = _sut.ReceiveAsync(Cts.Token);
            await Wait();

            Assert.False(waiting.IsCompleted);
            Assert.Equal(TestLock.State.Started, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.None, _receiveLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _sendLock.WaitState);
            Assert.Equal(TestLock.State.None, _sendLock.ReleaseState);
            _connection.Verify(x => x.ReceiveAsync(Cts.Token), Times.Never);

            _receiveLock.WaitComplete = true;
            await Wait();

            Assert.False(waiting.IsCompleted);
            Assert.Equal(TestLock.State.Completed, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.Started, _receiveLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _sendLock.WaitState);
            Assert.Equal(TestLock.State.None, _sendLock.ReleaseState);

            _receiveLock.ReleaseComplete = true;
            var result = await waiting;
            Assert.Equal(_message, result);

            Assert.True(waiting.IsCompletedSuccessfully);
            Assert.Equal(TestLock.State.Completed, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.Completed, _receiveLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _sendLock.WaitState);
            Assert.Equal(TestLock.State.None, _sendLock.ReleaseState);
        }

        [Fact]
        public async Task ShouldCancelOnSend()
        {
            var waiting = _sut.SendAsync(Create<object>(), Cts.Token);
            await Wait();

            Assert.False(waiting.IsCompleted);
            Assert.Equal(TestLock.State.Started, _sendLock.WaitState);
            Assert.Equal(TestLock.State.None, _sendLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.None, _receiveLock.ReleaseState);
            _connection.Verify(x => x.SendAsync(_message, Cts.Token), Times.Never);

            Cts.Cancel();
            await Wait();

            Assert.True(waiting.IsCanceled);
            Assert.Equal(TestLock.State.Canceled, _sendLock.WaitState);
            Assert.Equal(TestLock.State.None, _sendLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.None, _receiveLock.ReleaseState);
            _connection.Verify(x => x.SendAsync(_message, Cts.Token), Times.Never);
        }

        [Fact]
        public async Task ShouldCancelOnReceive()
        {
            var waiting = _sut.ReceiveAsync(Cts.Token);
            await Wait();

            Assert.False(waiting.IsCompleted);
            Assert.Equal(TestLock.State.Started, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.None, _receiveLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _sendLock.WaitState);
            Assert.Equal(TestLock.State.None, _sendLock.ReleaseState);
            _connection.Verify(x => x.ReceiveAsync(Cts.Token), Times.Never);

            Cts.Cancel();
            await Wait();

            Assert.True(waiting.IsCanceled);
            Assert.Equal(TestLock.State.Canceled, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.None, _receiveLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _sendLock.WaitState);
            Assert.Equal(TestLock.State.None, _sendLock.ReleaseState);
            _connection.Verify(x => x.ReceiveAsync(Cts.Token), Times.Never);
        }

        [Fact]
        public async Task ShouldReleaseProperlyWhenSendThrows()
        {
            var exception = Create<TestException>();
            _connection.Setup(x => x.SendAsync(It.IsAny<object>(), Cts.Token))
                .ThrowsAsync(exception);

            var waiting = _sut.SendAsync(Create<object>(), Cts.Token);
            await Wait();

            Assert.False(waiting.IsCompleted);
            Assert.Equal(TestLock.State.Started, _sendLock.WaitState);
            Assert.Equal(TestLock.State.None, _sendLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.None, _receiveLock.ReleaseState);

            _sendLock.WaitComplete = true;
            await Wait();

            Assert.False(waiting.IsCompleted);
            Assert.Equal(TestLock.State.Completed, _sendLock.WaitState);
            Assert.Equal(TestLock.State.Started, _sendLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.None, _receiveLock.ReleaseState);

            _sendLock.ReleaseComplete = true;
            await AssertThrowsAsync(() => waiting, exception);

            Assert.True(waiting.IsFaulted);
            Assert.Equal(TestLock.State.Completed, _sendLock.WaitState);
            Assert.Equal(TestLock.State.Completed, _sendLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.None, _receiveLock.ReleaseState);
        }

        [Fact]
        public async Task ShouldReleaseProperlyWhenReceiveThrows()
        {
            var exception = Create<TestException>();
            _connection.Setup(x => x.ReceiveAsync(Cts.Token))
                .ThrowsAsync(exception);

            var waiting = _sut.ReceiveAsync(Cts.Token);
            await Wait();

            Assert.False(waiting.IsCompleted);
            Assert.Equal(TestLock.State.Started, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.None, _receiveLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _sendLock.WaitState);
            Assert.Equal(TestLock.State.None, _sendLock.ReleaseState);

            _receiveLock.WaitComplete = true;
            await Wait();

            Assert.False(waiting.IsCompleted);
            Assert.Equal(TestLock.State.Completed, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.Started, _receiveLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _sendLock.WaitState);
            Assert.Equal(TestLock.State.None, _sendLock.ReleaseState);

            _receiveLock.ReleaseComplete = true;
            await AssertThrowsAsync(() => waiting.AsTask(), exception);

            Assert.True(waiting.IsFaulted);
            Assert.Equal(TestLock.State.Completed, _receiveLock.WaitState);
            Assert.Equal(TestLock.State.Completed, _receiveLock.ReleaseState);
            Assert.Equal(TestLock.State.None, _sendLock.WaitState);
            Assert.Equal(TestLock.State.None, _sendLock.ReleaseState);
        }
    }
}
