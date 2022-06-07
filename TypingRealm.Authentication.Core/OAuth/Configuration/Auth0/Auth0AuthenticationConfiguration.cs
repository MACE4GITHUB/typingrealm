namespace TypingRealm.Authentication.OAuth.Configuration;

public record Auth0AuthenticationConfiguration: AuthenticationProviderConfiguration
{
    public Auth0AuthenticationConfiguration(
        string issuer, string audience, string pkceClientId) : base(
        Audience: audience,
        Issuer: issuer,
        TokenEndpoint: $"{issuer}oauth/token",
        AuthorizationEndpoint: $"{issuer}authorize",
        PkceClientId: pkceClientId,
        ServiceClientId: null,
        ServiceClientSecret: null,
        SuppressErrorOnDiscovery: true)
    { }
}
