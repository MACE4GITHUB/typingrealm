using System;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace TypingRealm.Authentication
{
    public sealed class AuthenticationInformation
    {
        public string? Issuer { get; set; }
        public string? AuthorizationEndpoint { get; set; }
        public string? TokenEndpoint { get; set; }

        // TODO: Get client ID and secret from secret store, separate for different providers.
        public string? ServiceClientId { get; set; }
        public string? ServiceClientSecret { get; set; }

        // This can be set for false for local IdentityServer that will not be exposed outside the Docker/Cluster network.
        public bool RequireHttpsMetadata { get; set; }

        public TokenValidationParameters TokenValidationParameters { get; }
            = new TokenValidationParameters
            {
                NameClaimType = ClaimTypes.NameIdentifier,
                ClockSkew = TimeSpan.Zero
            };
    }
}
