using System;
using System.Threading.Tasks;
using Xunit;

namespace TypingRealm.Tests;

public class TestAsyncManagedDisposable : AsyncManagedDisposable
{
    public bool IsDisposed { get; set; }
    public int WaitBeforeDisposingMs { get; set; }

    protected override async ValueTask DisposeManagedResourcesAsync()
    {
        if (WaitBeforeDisposingMs > 0)
            await Task.Delay(WaitBeforeDisposingMs);

        IsDisposed = true;
    }
}

public class AsyncManagedDisposableTests : IDisposable
{
    private readonly TestAsyncManagedDisposable _sut;

    public AsyncManagedDisposableTests()
    {
        _sut = new TestAsyncManagedDisposable();
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    [Fact]
    public void ShouldInheritManagedDisposable()
    {
        Assert.IsAssignableFrom<ManagedDisposable>(_sut);
    }

    [Fact]
    public void ShouldDisposeSync()
    {
        _sut.Dispose();
        Assert.True(_sut.IsDisposed);
    }

    [Fact]
    public async Task ShouldDisposeAsync()
    {
        await _sut.DisposeAsync();
        Assert.True(_sut.IsDisposed);
    }

    [Fact]
    public async Task ShouldDisposeSyncAfterWaiting()
    {
        _sut.WaitBeforeDisposingMs = 100;
        _sut.Dispose();
        Assert.True(_sut.IsDisposed);
    }
}
