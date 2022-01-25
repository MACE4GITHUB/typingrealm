using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging.Connections;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connections;

public class SemaphoreSlimLockTests : TestsBase
{
    [Theory, AutoMoqData]
    public void ShouldBeSyncManagedDisposable(SemaphoreSlimLock sut)
    {
        Assert.IsAssignableFrom<SyncManagedDisposable>(sut);
    }

    [Theory, AutoMoqData]
    public async Task WaitAsync_ShouldThrowIfDisposed(SemaphoreSlimLock sut)
    {
        sut.Dispose();

        var exception = await AssertThrowsAsync<ObjectDisposedException>(
            () => sut.WaitAsync(Cts.Token));

        Assert.Equal(typeof(SemaphoreSlimLock).FullName, exception.ObjectName);
    }

    [Theory, AutoMoqData]
    public async Task ReleaseAsync_ShouldThrowIfDisposed(SemaphoreSlimLock sut)
    {
        sut.Dispose();

        var exception = await AssertThrowsAsync<ObjectDisposedException>(
            () => sut.ReleaseAsync(Cts.Token));

        Assert.Equal(typeof(SemaphoreSlimLock).FullName, exception.ObjectName);
    }

    [Theory, AutoMoqData]
    public async Task ShouldWaitAndRelease(SemaphoreSlimLock sut)
    {
        var mutex = GetSemaphore(sut);
        Assert.Equal(1, mutex.CurrentCount);

        await sut.WaitAsync(Cts.Token);
        Assert.Equal(0, mutex.CurrentCount);

        var second = sut.WaitAsync(Cts.Token);
        await Wait();
        Assert.False(second.IsCompleted);
        Assert.Equal(0, mutex.CurrentCount);

        await sut.ReleaseAsync(Cts.Token);
        await second;
        Assert.True(second.IsCompletedSuccessfully);
        Assert.Equal(0, mutex.CurrentCount);

        await sut.ReleaseAsync(Cts.Token);
        Assert.Equal(1, mutex.CurrentCount);

        await AssertThrowsAsync<SemaphoreFullException>(
            () => sut.ReleaseAsync(Cts.Token));
    }

    [Theory, AutoMoqData]
    public async Task ShouldCancelWaiting(SemaphoreSlimLock sut)
    {
        var mutex = GetSemaphore(sut);
        await sut.WaitAsync(Cts.Token);

        var second = sut.WaitAsync(Cts.Token);
        await Wait();
        Assert.False(second.IsCompleted);
        Assert.Equal(0, mutex.CurrentCount);

        Cts.Cancel();
        await AssertThrowsAsync<OperationCanceledException>(
            () => second);
        Assert.True(second.IsCanceled);
        Assert.Equal(0, mutex.CurrentCount);
    }

    [Theory, AutoMoqData]
    public async Task ShouldDisposeOfSemaphoreSlim(SemaphoreSlimLock sut)
    {
        sut.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => GetSemaphore(sut).WaitAsync());
    }

    private SemaphoreSlim GetSemaphore(SemaphoreSlimLock sut)
    {
        return (SemaphoreSlim)GetPrivateField(sut, "_mutex")!;
    }
}
