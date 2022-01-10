using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Authentication.OAuth;

namespace TypingRealm.Authentication
{
    /// <summary>
    /// Issues a new Service token for S2S communication or gets a cached one.
    /// If you need to get current service token from HTTP request, use
    /// <see cref="IProfileTokenService"/>. It is still named Service and not
    /// Issuer, because in future it's implementation might even take this token
    /// from the Http request if we decide to send both user and client tokens
    /// in separate headers.
    /// </summary>
    public interface IServiceTokenService
    {
        ValueTask<string> GetServiceAccessTokenAsync(CancellationToken cancellationToken);
        ValueTask<string> GetServiceAccessTokenAsync(ClientCredentials credentials, CancellationToken cancellationToken);
    }

    // It needs to be public because it is used by API.
    // TODO: Split this entity and API entity, make fields CamelCase with serialization attributes.
    public sealed record TokenResponse
    {
        public string? access_token { get; set; }
        public string? scope { get; set; }
        public int expires_in { get; set; }
        public string? token_type { get; set; }
    }

    public sealed class ServiceTokenService : IServiceTokenService
    {
        private readonly IAuthenticationInformationProvider _authenticationInformationProvider;

        public ServiceTokenService(IAuthenticationInformationProvider authenticationInformationProvider)
        {
            _authenticationInformationProvider = authenticationInformationProvider;
        }

        public ValueTask<string> GetServiceAccessTokenAsync(CancellationToken cancellationToken)
        {
            var authenticationInformation = _authenticationInformationProvider.GetServiceAuthenticationInformation();
            if (authenticationInformation.ServiceClientId == null || authenticationInformation.ServiceClientSecret == null)
                throw new InvalidOperationException("Client credentials authentication information is not set for service.");

            return GetServiceAccessTokenAsync(new ClientCredentials(
                authenticationInformation.ServiceClientId,
                authenticationInformation.ServiceClientSecret,
                Enumerable.Empty<string>()), cancellationToken);
        }

        public async ValueTask<string> GetServiceAccessTokenAsync(ClientCredentials credentials, CancellationToken cancellationToken)
        {
            var authenticationInformation = _authenticationInformationProvider.GetServiceAuthenticationInformation();

            using var httpClient = new HttpClient();
            var parameters = new List<KeyValuePair<string?, string?>>
            {
                new KeyValuePair<string?, string?>("client_id", credentials.ClientId),
                new KeyValuePair<string?, string?>("client_secret", credentials.ClientSecret),
                new KeyValuePair<string?, string?>("audience", "https://api.typingrealm.com"),
                new KeyValuePair<string?, string?>("grant_type", "client_credentials")
            };

            if (credentials.Scopes.Any())
                parameters.Add(new("scope", string.Join(' ', credentials.Scopes)));

            using var content = new FormUrlEncodedContent(parameters);

            var response = await httpClient.PostAsync(authenticationInformation.TokenEndpoint, content, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);
            var tokenData = JsonSerializer.Deserialize<TokenResponse>(json);

            if (tokenData?.access_token == null)
                throw new InvalidOperationException("Access token is null, invalid conversion from token endpoint response.");

            // TODO: Cache this token and renew only when expired / about to expire.
            // Make sure caching is registered as singletone as this class is transient as of now.
            return tokenData.access_token;
        }
    }
}
