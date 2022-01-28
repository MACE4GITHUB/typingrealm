using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace TypingRealm.Tests;

public class TestManagedDisposable : ManagedDisposable
{
    public bool IsDisposedTest { get; set; }
    public bool IsDisposedAsyncTest { get; set; }

    public void PublicThrowIfDisposed() => ThrowIfDisposed();

    protected override void DisposeManagedResources()
    {
        IsDisposedTest = true;
    }

    protected override ValueTask DisposeManagedResourcesAsync()
    {
        IsDisposedAsyncTest = true;
        return default;
    }
}

public class ManagedDisposableTests : IDisposable
{
    private readonly TestManagedDisposable _sut;

    public ManagedDisposableTests()
    {
        _sut = new TestManagedDisposable();

        // Valid initial state.
        Assert.False(_sut.IsDisposedTest);
        Assert.False(_sut.IsDisposedAsyncTest);
        _sut.PublicThrowIfDisposed(); // Doesn't throw.
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    [Fact]
    public void ThrowIfDisposed_ShouldNotBeVirtual() // So we can test it only here.
    {
        Assert.False(typeof(ManagedDisposable).GetMethod("ThrowIfDisposed", BindingFlags.Instance | BindingFlags.NonPublic)?.IsVirtual);
    }

    [Fact]
    public void ShouldDisposeOnlySync()
    {
        _sut.Dispose();

        Assert.True(_sut.IsDisposedTest);
        Assert.False(_sut.IsDisposedAsyncTest);
    }

    [Fact]
    public async Task ShouldDisposeOnlyAsync()
    {
        await _sut.DisposeAsync();

        Assert.False(_sut.IsDisposedTest);
        Assert.True(_sut.IsDisposedAsyncTest);
    }

    [Fact]
    public async Task ShouldNotDisposeAsyncAfterSync()
    {
#pragma warning disable CA1849 // Call async methods when in an async method
        _sut.Dispose();
#pragma warning restore CA1849

        _sut.IsDisposedTest = false;

        await _sut.DisposeAsync();

        Assert.False(_sut.IsDisposedTest);
        Assert.False(_sut.IsDisposedAsyncTest);
    }

    [Fact]
    public async Task ShouldNotDisposeSyncAfterAsync()
    {
        await _sut.DisposeAsync();
        _sut.IsDisposedAsyncTest = false;

#pragma warning disable CA1849 // Call async methods when in an async method
        _sut.Dispose();
#pragma warning restore CA1849

        Assert.False(_sut.IsDisposedTest);
        Assert.False(_sut.IsDisposedAsyncTest);
    }

    [Fact]
    public void ShouldNotDisposeSyncMultipleTimes()
    {
        _sut.Dispose();
        _sut.IsDisposedTest = false;

        _sut.Dispose();
        Assert.False(_sut.IsDisposedTest);
    }

    [Fact]
    public async Task ShouldNotDisposeAsyncMultipleTimes()
    {
        await _sut.DisposeAsync();
        _sut.IsDisposedAsyncTest = false;

        await _sut.DisposeAsync();
        Assert.False(_sut.IsDisposedAsyncTest);
    }

    [Fact]
    public void ShouldThrowOnActionWhenAlreadyDisposedSync()
    {
        _sut.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _sut.PublicThrowIfDisposed());
    }

    [Fact]
    public async Task ShouldThrowOnActionWhenAlreadyDisposedAsync()
    {
        await _sut.DisposeAsync();

        Assert.Throws<ObjectDisposedException>(() => _sut.PublicThrowIfDisposed());
    }

    [Fact]
    public async Task ShouldHaveIsDisposedFalse_WhenNotDisposed()
    {
        Assert.False(_sut.IsDisposedTest);
    }

    [Fact]
    public void ShouldHaveIsDisposedTrue_AfterDisposedSync()
    {
        _sut.Dispose();

        Assert.True(_sut.IsDisposed);
    }

    [Fact]
    public async Task ShouldHaveIsDisposedTrue_AfterDisposedAsync()
    {
        await _sut.DisposeAsync();

        Assert.True(_sut.IsDisposed);
    }
}
