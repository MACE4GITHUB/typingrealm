using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace TypingRealm.Tests;

public class TestManagedDisposable : ManagedDisposable
{
    public bool IsDisposed { get; set; }
    public bool IsDisposedAsync { get; set; }

    public void PublicThrowIfDisposed() => ThrowIfDisposed();

    protected override void DisposeManagedResources()
    {
        IsDisposed = true;
    }

    protected override ValueTask DisposeManagedResourcesAsync()
    {
        IsDisposedAsync = true;
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
        Assert.False(_sut.IsDisposed);
        Assert.False(_sut.IsDisposedAsync);
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

        Assert.True(_sut.IsDisposed);
        Assert.False(_sut.IsDisposedAsync);
    }

    [Fact]
    public async Task ShouldDisposeOnlyAsync()
    {
        await _sut.DisposeAsync();

        Assert.False(_sut.IsDisposed);
        Assert.True(_sut.IsDisposedAsync);
    }

    [Fact]
    public async Task ShouldNotDisposeAsyncAfterSync()
    {
        _sut.Dispose();
        _sut.IsDisposed = false;

        await _sut.DisposeAsync();

        Assert.False(_sut.IsDisposed);
        Assert.False(_sut.IsDisposedAsync);
    }

    [Fact]
    public async Task ShouldNotDisposeSyncAfterAsync()
    {
        await _sut.DisposeAsync();
        _sut.IsDisposedAsync = false;

        _sut.Dispose();

        Assert.False(_sut.IsDisposed);
        Assert.False(_sut.IsDisposedAsync);
    }

    [Fact]
    public void ShouldNotDisposeSyncMultipleTimes()
    {
        _sut.Dispose();
        _sut.IsDisposed = false;

        _sut.Dispose();
        Assert.False(_sut.IsDisposed);
    }

    [Fact]
    public async Task ShouldNotDisposeAsyncMultipleTimes()
    {
        await _sut.DisposeAsync();
        _sut.IsDisposedAsync = false;

        await _sut.DisposeAsync();
        Assert.False(_sut.IsDisposedAsync);
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
}
