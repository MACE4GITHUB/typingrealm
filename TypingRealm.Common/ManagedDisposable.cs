using System;
using System.Threading.Tasks;

namespace TypingRealm
{
    /// <summary>
    /// Helper class for easier implementation of Disposable classes that
    /// contain only managed resources.
    /// Override <see cref="DisposeManagedResources"/> method and clear all
    /// managed resources inside it.
    /// If you want to have a custom <see cref="DisposeAsync"/> implementation -
    /// override <see cref="DisposeManagedResourcesAsync"/> method as well and
    /// do NOT call "base.DisposeManagedResourcesAsync" inside it as the default
    /// implementation calls synchronous version.
    /// Use <see cref="ThrowIfDisposed"/> method on all methods that shouldn't
    /// work when the object is already disposed.
    /// </summary>
#pragma warning disable S3881, CA1063, CA1816 // Disposable pattern.
    public abstract class ManagedDisposable : IDisposable, IAsyncDisposable
    {
        private bool _isDisposed;

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            await DisposeManagedResourcesAsync().ConfigureAwait(false);
            _isDisposed = true;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            DisposeManagedResources();
            _isDisposed = true;
        }

        protected void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected abstract ValueTask DisposeManagedResourcesAsync();
        protected abstract void DisposeManagedResources();
    }
#pragma warning restore S3881, CA1063, CA1816

    public abstract class SyncManagedDisposable : ManagedDisposable
    {
        protected override ValueTask DisposeManagedResourcesAsync()
        {
            DisposeManagedResources();
            return default;
        }
    }

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
}
