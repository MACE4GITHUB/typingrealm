namespace TypingRealm.Authentication.OAuth.Configuration;

public sealed record DevelopmentAuth0AuthenticationConfiguration : Auth0AuthenticationConfiguration
{
    private const string DevTenant = "https://dev-typingrealm.eu.auth0.com/";

    public DevelopmentAuth0AuthenticationConfiguration() : base(
        issuer: DevTenant,
        audience: "https://dev.api.typingrealm.com",
        pkceClientId: "MmL3eIAJPW7wweAWajjqgWRM8xaVqRn2")
    { }
}
