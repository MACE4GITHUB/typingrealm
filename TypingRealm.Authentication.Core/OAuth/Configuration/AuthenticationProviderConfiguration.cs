namespace TypingRealm.Authentication.OAuth.Configuration;

public abstract record AuthenticationProviderConfiguration(
    string Audience,
    string Issuer,
    string TokenEndpoint,
    string AuthorizationEndpoint,
    string PkceClientId,
    string? ServiceClientId,
    string? ServiceClientSecret,
    bool SuppressErrorOnDiscovery)
{
    public bool RequireHttpsMetadata { get; set; } = true;
}
