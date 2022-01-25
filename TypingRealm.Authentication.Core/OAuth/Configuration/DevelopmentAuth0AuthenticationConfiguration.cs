namespace TypingRealm.Authentication.OAuth.Configuration;

public sealed record DevelopmentAuth0AuthenticationConfiguration : AuthenticationProviderConfiguration
{
    private const string Auth0Issuer = "https://dev-typingrealm.eu.auth0.com/";

    public DevelopmentAuth0AuthenticationConfiguration() : base(
        Audience: "https://dev.api.typingrealm.com",
        Issuer: Auth0Issuer,
        TokenEndpoint: $"{Auth0Issuer}oauth/token",
        AuthorizationEndpoint: $"{Auth0Issuer}authorize",
        PkceClientId: "MmL3eIAJPW7wweAWajjqgWRM8xaVqRn2",
        ServiceClientId: "no_client_id",
        ServiceClientSecret: "no_client_secret")
    { }
}
