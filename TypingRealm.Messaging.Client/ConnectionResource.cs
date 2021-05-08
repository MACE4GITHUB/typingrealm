using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Client
{
    public sealed class ConnectionResource : AsyncManagedDisposable
    {
        private readonly CancellationTokenSource _cts;
        private readonly ConnectionWithDisconnect _connectionWithDisconnect;

        public ConnectionResource(
            ConnectionWithDisconnect connectionWithDisconnect,
            string characterId,
            CancellationToken originalCancellationToken)
        {
            _connectionWithDisconnect = connectionWithDisconnect;
            CharacterId = characterId;
            OriginalCancellationToken = originalCancellationToken;

            _cts = new CancellationTokenSource();
            CombinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                originalCancellationToken,
                _cts.Token);
        }

        public string CharacterId { get; }
        public CancellationToken OriginalCancellationToken { get; }
        public CancellationTokenSource CombinedCts { get; }

        private Task? _listening;
        public Task Listening => _listening ?? throw new InvalidOperationException("Listening has not been set.");

        public async ValueTask UseCombinedCts(Func<CancellationToken, ValueTask> action, CancellationToken cancellationToken)
        {
            using var cts = new CancellationTokenSource();
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(CombinedCts.Token, cancellationToken);

            await action(combinedCts.Token)
                .ConfigureAwait(false);
        }

        public void SetListening(Func<CancellationToken, Task> listening)
        {
            _listening = listening(CombinedCts.Token);
        }

        public IConnection Connection => _connectionWithDisconnect.Connection;

        protected override async ValueTask DisposeManagedResourcesAsync()
        {
            _cts.Cancel();

            // TODO: Consider catching and logging exceptions, do not fail execution.
            await Listening.ConfigureAwait(false);
            await _connectionWithDisconnect.DisconnectAsync()
                .ConfigureAwait(false);

            _cts.Dispose();
            CombinedCts.Dispose();
        }
    }
}
