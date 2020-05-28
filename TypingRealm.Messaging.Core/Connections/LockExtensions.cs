using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Connections
{
    public static class LockExtensions
    {
        /// <summary>
        /// Waits to acquire the lock and returns <see cref="IAsyncDisposable"/>
        /// instance after the lock is acquired, which can be disposed in order
        /// to release lock instance.
        /// </summary>
        public static async ValueTask<IAsyncDisposable> UseWaitAsync(this ILock @lock, CancellationToken cancellationToken)
        {
            await @lock.WaitAsync(cancellationToken).ConfigureAwait(false);

            return new LockReleaseWrapper(@lock, cancellationToken);
        }

        private sealed class LockReleaseWrapper : AsyncManagedDisposable
        {
            private readonly ILock _lock;
            private readonly CancellationToken _cancellationToken;

            public LockReleaseWrapper(ILock @lock, CancellationToken cancellationToken)
            {
                _lock = @lock;
                _cancellationToken = cancellationToken;
            }

            protected override ValueTask DisposeManagedResourcesAsync()
                => _lock.ReleaseAsync(_cancellationToken);
        }
    }
}
