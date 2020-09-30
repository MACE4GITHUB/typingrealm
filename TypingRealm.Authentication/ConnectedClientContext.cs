using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;

namespace TypingRealm.Authentication
{
    public sealed class ConnectedClientContext : AsyncManagedDisposable, IConnectedClientContext
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private ConnectedClient? _connectedClient;
        private ClaimsPrincipal? _claimsPrincipal;
        private JwtSecurityToken? _securityToken;
        private Task? _notifyAboutExpiration;

        public string GetAccessToken() => _securityToken?.RawData ?? throw new InvalidOperationException("Access token is not set.");

        public void SetAuthenticatedContext(ClaimsPrincipal claimsPrincipal, JwtSecurityToken securityToken)
        {
            if (claimsPrincipal == null)
                throw new ArgumentNullException(nameof(claimsPrincipal));

            if (securityToken == null)
                throw new ArgumentNullException(nameof(securityToken));

            _claimsPrincipal = claimsPrincipal;
            _securityToken = securityToken;
        }

        public void SetConnectedClient(ConnectedClient connectedClient)
        {
            if (_claimsPrincipal == null || _securityToken == null)
                throw new InvalidOperationException("Token is not set.");

            if (connectedClient == null)
                throw new ArgumentNullException(nameof(connectedClient));

            _connectedClient = connectedClient;
            _notifyAboutExpiration = NotifyAboutExpirationAsync(_cts.Token);
        }

        protected override async ValueTask DisposeManagedResourcesAsync()
        {
            _cts.Cancel();

            if (_notifyAboutExpiration != null)
                await _notifyAboutExpiration.ConfigureAwait(false);

            _cts.Dispose();
        }

        private async Task NotifyAboutExpirationAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

                if (_connectedClient == null || _securityToken == null)
                    continue;

                if (DateTime.UtcNow > _securityToken.ValidTo - TimeSpan.FromMinutes(1))
                {
                    await _connectedClient.Connection.SendAsync(new TokenExpired(), cancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }
    }
}
