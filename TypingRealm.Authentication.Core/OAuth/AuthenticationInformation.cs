using System;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace TypingRealm.Authentication.OAuth;

/// <summary>
/// This class data is used to configure Web API Authentication / configure
/// .NET JWT validators to manually validate JWT tokens in services where needed.
/// </summary>
public sealed class AuthenticationInformation
{
    public string? SchemeName { get; set; }

    public string? Issuer { get; set; }
    public string? AuthorizationEndpoint { get; set; }
    public string? TokenEndpoint { get; set; }

    // TODO: Get client ID and secret from secret store, separate for different providers.
    public string? ServiceClientId { get; set; }
    public string? ServiceClientSecret { get; set; }

    // This can be set for false for local IdentityServer that will not be exposed outside the Docker/Cluster network.
    public bool RequireHttpsMetadata { get; set; } = true;

    // This can be set to suppress error on Discovery, like when Auth0 doesn't work.
    public bool SuppressErrorOnDiscovery { get; set; }

    public TokenValidationParameters TokenValidationParameters { get; }
        = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.NameIdentifier,
            ClockSkew = TimeSpan.Zero
        };
}

/// <summary>
/// This class is used solely to validate AuthenticationInformation.
/// </summary>
public sealed class AuthenticationInformationBuilder
{
    public AuthenticationInformation AuthenticationInformation { get; }
        = new AuthenticationInformation();

    public AuthenticationInformation Build(string schemeName)
    {
        AuthenticationInformation.SchemeName = schemeName;

        if (string.IsNullOrEmpty(AuthenticationInformation.SchemeName)
            || string.IsNullOrEmpty(AuthenticationInformation.Issuer)
            || string.IsNullOrEmpty(AuthenticationInformation.AuthorizationEndpoint)
            || string.IsNullOrEmpty(AuthenticationInformation.TokenEndpoint))
            //|| string.IsNullOrEmpty(AuthenticationInformation.ServiceClientId)
            //|| string.IsNullOrEmpty(AuthenticationInformation.ServiceClientSecret))
            throw new InvalidOperationException("Authentication information has empty fields.");

        return AuthenticationInformation;
    }
}
