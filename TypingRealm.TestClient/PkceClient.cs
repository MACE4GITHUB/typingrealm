using System;
using System.Threading.Tasks;
using IdentityModel.OidcClient;
using TypingRealm.Messaging.Connections;

namespace TypingRealm.TestClient
{
    public sealed class PkceClient : SyncManagedDisposable
    {
        private readonly OidcClient _oidcClient;
        private readonly SemaphoreSlimLock _lock = new SemaphoreSlimLock();

        private DateTime _accessTokenExpiration;
        private string? _accessToken;
        private string? _identityToken;
        private string? _refreshToken;

        public PkceClient(string authority, string clientId)
        {
            var browser = new SystemBrowser(4200);
            var redirectUri = $"http://127.0.0.1:{browser.Port}";

            var options = new OidcClientOptions
            {
                Authority = authority,
                ClientId = clientId,
                RedirectUri = redirectUri,
                Scope = "openid profile offline_access",
                FilterClaims = false,
                Browser = browser
            };

            options.Policy.RequireIdentityTokenSignature = false;

            _oidcClient = new OidcClient(options);
        }

        public async Task<string> SignInAsync()
        {
            await using var @lock = await _lock.UseWaitAsync(default).ConfigureAwait(false);

            // TODO: Use UTC.
            if (_accessTokenExpiration > DateTime.Now.AddMinutes(1) && _accessToken != null)
                return _accessToken;

            if (_refreshToken != null)
            {
                try
                {
                    var refreshResult = await _oidcClient.RefreshTokenAsync(_refreshToken).ConfigureAwait(false);
                    if (refreshResult.RefreshToken != null)
                        _refreshToken = refreshResult.RefreshToken;
                    if (refreshResult.IdentityToken != null)
                        _identityToken = refreshResult.IdentityToken;

                    _accessToken = refreshResult.AccessToken;
                    _accessTokenExpiration = refreshResult.AccessTokenExpiration;

                    return _accessToken;
                }
                catch
                {
                    // TODO: Log the exception, and then follow to the next part to try to log in again.
                }
            }

            var loginResult = await _oidcClient.LoginAsync(new LoginRequest()).ConfigureAwait(false);
            if (loginResult.RefreshToken != null)
                _refreshToken = loginResult.RefreshToken;
            if (loginResult.IdentityToken != null)
                _identityToken = loginResult.IdentityToken;

            _accessToken = loginResult.AccessToken;
            _accessTokenExpiration = loginResult.AccessTokenExpiration;

            return _accessToken;
        }

        protected override void DisposeManagedResources()
        {
            _lock.Dispose();
        }
    }
}
