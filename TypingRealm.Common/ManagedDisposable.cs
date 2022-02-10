using System;
using System.Threading.Tasks;

namespace TypingRealm;

/// <summary>
/// Helper class for easier implementation of Disposable classes that
/// contain only managed resources.
/// Override <see cref="DisposeManagedResources"/> and
/// <see cref="DisposeManagedResourcesAsync"/> methods and clear all managed
/// resources inside it.
/// Use <see cref="ThrowIfDisposed"/> method on all methods that shouldn't
/// work when the object has already been disposed.
/// </summary>
public abstract class ManagedDisposable : IDisposable, IAsyncDisposable
{
    public bool IsDisposed { get; private set; }

    public async ValueTask DisposeAsync()
    {
        if (IsDisposed)
            return;

        await DisposeManagedResourcesAsync().ConfigureAwait(false);
        IsDisposed = true;
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;

        DisposeManagedResources();
        IsDisposed = true;
    }

    protected void ThrowIfDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    protected abstract ValueTask DisposeManagedResourcesAsync();
    protected abstract void DisposeManagedResources();
}

/// <summary>
/// Use this class when resources are disposed synchronously.
/// </summary>
public abstract class SyncManagedDisposable : ManagedDisposable
{
    protected override ValueTask DisposeManagedResourcesAsync()
    {
        DisposeManagedResources();
        return default;
    }
}

/// <summary>
/// Use this class when resources are disposed asynchronously.
/// </summary>
public abstract class AsyncManagedDisposable : ManagedDisposable
{
    protected override void DisposeManagedResources()
    {
        var dispose = DisposeManagedResourcesAsync();
        if (dispose.IsCompletedSuccessfully)
            return;

        dispose.AsTask().GetAwaiter().GetResult();
    }
}
