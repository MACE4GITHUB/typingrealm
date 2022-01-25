namespace TypingRealm.Authentication.OAuth.Configuration;

public sealed record Auth0AuthenticationConfiguration : AuthenticationProviderConfiguration
{
    private const string Auth0Issuer = "https://typingrealm.us.auth0.com/";

    public Auth0AuthenticationConfiguration() : base(
        Audience: "https://api.typingrealm.com",
        Issuer: Auth0Issuer,
        TokenEndpoint: $"{Auth0Issuer}oauth/token",
        AuthorizationEndpoint: $"{Auth0Issuer}authorize",
        PkceClientId: "usmQTpTvmVrxC4QtYMYj6R7aIa6Ambck",
        ServiceClientId: "no_client_id",
        ServiceClientSecret: "no_client_secret")
    { }
}
