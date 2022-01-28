namespace TypingRealm.Authentication.OAuth.Configuration;

public sealed record ProductionAuth0AuthenticationConfiguration : Auth0AuthenticationConfiguration
{
    private const string ProductionTenant = "https://typingrealm.us.auth0.com/";

    public ProductionAuth0AuthenticationConfiguration() : base(
        issuer: ProductionTenant,
        audience: "https://api.typingrealm.com",
        pkceClientId: "usmQTpTvmVrxC4QtYMYj6R7aIa6Ambck")
    { }
}
