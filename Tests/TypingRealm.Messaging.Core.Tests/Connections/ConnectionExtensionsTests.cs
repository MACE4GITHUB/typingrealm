using TypingRealm.Messaging.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connections
{
    public class ConnectionExtensionsTests : TestsBase
    {
        [Theory, AutoMoqData]
        public void WithLocking_ShouldWrapConnectionInLockingConnection(
            IConnection connection, ILock sendLock, ILock receiveLock)
        {
            var sut = connection.WithLocking(sendLock, receiveLock);

            Assert.IsType<LockingConnection>(sut);
            Assert.Equal(connection, GetPrivateField(sut, "_connection"));
            Assert.Equal(sendLock, GetPrivateField(sut, "_sendLock"));
            Assert.Equal(receiveLock, GetPrivateField(sut, "_receiveLock"));
        }
    }
}
