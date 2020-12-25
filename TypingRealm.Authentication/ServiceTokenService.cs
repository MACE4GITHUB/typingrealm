﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Authentication
{
    public sealed class ServiceTokenService : IServiceTokenService
    {
        private readonly AuthenticationInformation _authenticationInformation;

        public ServiceTokenService(AuthenticationInformation authenticationInformation)
        {
            _authenticationInformation = authenticationInformation;
        }

        public async ValueTask<string> GetServiceAccessTokenAsync(CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();
            using var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string?, string?>("client_id", _authenticationInformation.ServiceClientId),
                new KeyValuePair<string?, string?>("client_secret", _authenticationInformation.ServiceClientSecret),
                new KeyValuePair<string?, string?>("audience", "https://api.typingrealm.com"),
                new KeyValuePair<string?, string?>("grant_type", "client_credentials")
            });

            var response = await httpClient.PostAsync(_authenticationInformation.TokenEndpoint, content)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var tokenData = JsonSerializer.Deserialize<TokenResponse>(json);

            if (tokenData?.access_token == null)
                throw new InvalidOperationException("Access token is null, invalid conversion from token endpoint response.");

            // TODO: Cache this token and renew only when expired / about to expire.
            return tokenData.access_token;
        }
    }
}
