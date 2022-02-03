using TypingRealm.Messaging.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connections;

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

    [Theory, AutoMoqData]
    public void WithNotificator_ShouldWrapConnectionInNotificatonConnection(
        Notificator notificator, IMessageSender messageSender)
    {
        var sut = messageSender.WithNotificator(notificator);

        Assert.IsType<NotificatorConnection>(sut);
        Assert.Equal(notificator, GetPrivateField(sut, "_notificator"));
        Assert.Equal(messageSender, GetPrivateField(sut, "_messageSender"));
    }
}
