using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connections;

public class NotificatorConnectionTests : TestsBase
{
    [Theory, AutoMoqData]
    public async Task ShouldUseConnectionToSendMessages(
        [Frozen] Mock<IMessageSender> messageSender,
        TestMessage message,
        NotificatorConnection sut)
    {
        await sut.SendAsync(message, Cts.Token);

        messageSender.Verify(x => x.SendAsync(message, Cts.Token));
    }

#pragma warning disable CA2012 // Use ValueTasks correctly
#pragma warning disable S5034 // "ValueTask" should be consumed correctly
    [Theory, AutoMoqData]
    public void ShouldReturnMessageSynchronouslyIfAlreadyReceived(
        [Frozen] Notificator notificator,
        NotificatorConnection sut)
    {
        notificator.NotifyReceived(Create<object>());
        Assert.True(sut.ReceiveAsync(Cts.Token).IsCompletedSuccessfully);

        var message = Create<object>();
        notificator.NotifyReceived(message);
        Assert.Equal(message, sut.ReceiveAsync(Cts.Token).Result);
    }
#pragma warning restore CA2012
#pragma warning restore S5034

    [Theory, AutoMoqData]
    public async Task ShouldReturnMessageWhenNotified(
        [Frozen] Notificator notificator,
        TestMessage message,
        NotificatorConnection sut)
    {
        var result = sut.ReceiveAsync(Cts.Token);
        await Wait();

        Assert.False(result.IsCompleted);

        notificator.NotifyReceived(message);
        var received = await result;

        Assert.Equal(message, received);
    }

    [Theory, AutoMoqData]
    public async Task ShouldCancel_WhenCancellationRequested(NotificatorConnection sut)
    {
        var result = sut.ReceiveAsync(Cts.Token);
        await Wait();

        Assert.False(result.IsCompleted);

        Cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => result.AsTask());
    }

    [Theory, AutoMoqData]
    public async Task ShouldUnsubscribeAfterNotification(
        [Frozen] Notificator notificator,
        NotificatorConnection sut)
    {
        int GetSubscriptionCount()
        {
            return (typeof(Notificator)
            .GetField(nameof(Notificator.Received), BindingFlags.Instance | BindingFlags.NonPublic)?
            .GetValue(notificator) as Action)?.GetInvocationList()?.Length ?? 0;
        }

        Assert.Equal(0, GetSubscriptionCount());

        var result = sut.ReceiveAsync(Cts.Token);
        await Wait();

        Assert.Equal(1, GetSubscriptionCount());

        notificator.NotifyReceived(Create<object>());
        await result;

        Assert.Equal(0, GetSubscriptionCount());
    }

    [Theory, AutoMoqData]
    public async Task ShouldUnsubscribeAfterCancellation(
        [Frozen] Notificator notificator,
        NotificatorConnection sut)
    {
        int GetSubscriptionCount()
        {
            return (typeof(Notificator)
            .GetField(nameof(Notificator.Received), BindingFlags.Instance | BindingFlags.NonPublic)?
            .GetValue(notificator) as Action)?.GetInvocationList()?.Length ?? 0;
        }

        Assert.Equal(0, GetSubscriptionCount());

        var result = sut.ReceiveAsync(Cts.Token);
        await Wait();

        Assert.Equal(1, GetSubscriptionCount());

        Cts.Cancel();
        await SwallowAnyAsync(result.AsTask());

        Assert.Equal(0, GetSubscriptionCount());
    }

    [Theory, AutoMoqData]
    public async Task ShouldWorkInHighConcurrencyScenario(
        [Frozen] Notificator notificator,
        NotificatorConnection sut)
    {
        var notifyTask = Task.WhenAll(Enumerable.Range(0, 1000).Select(_ => Task.Run(async () =>
        {
            await Wait(RandomNumberGenerator.GetInt32(100, 150));
            notificator.NotifyReceived("message");
        })));

        var processingTask = Task.Run(async () =>
        {
            for (var i = 0; i < 1000; i++)
            {
                Assert.Equal("message", await sut.ReceiveAsync(Cts.Token));
            }
        });

        await Task.WhenAll(notifyTask, processingTask);
    }
}
