using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm;

public sealed class SemaphoreSlimLock : SyncManagedDisposable, ILock
{
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public ValueTask ReleaseAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        _mutex.Release();
        return default;
    }

    public async ValueTask WaitAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        await _mutex.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override void DisposeManagedResources()
    {
        _mutex.Dispose();
    }
}
