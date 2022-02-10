using System;
using System.Threading.Tasks;
using Xunit;

namespace TypingRealm.Tests;

public class TestSyncManagedDisposable : SyncManagedDisposable
{
    public bool TestIsDisposed { get; set; }

    protected override void DisposeManagedResources()
    {
        TestIsDisposed = true;
    }
}

public class SyncManagedDisposableTests : IDisposable
{
    private readonly TestSyncManagedDisposable _sut;

    public SyncManagedDisposableTests()
    {
        _sut = new TestSyncManagedDisposable();
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
        Assert.True(_sut.TestIsDisposed);
    }

    [Fact]
    public async Task ShouldDisposeAsync()
    {
        await _sut.DisposeAsync();
        Assert.True(_sut.TestIsDisposed);
    }
}
