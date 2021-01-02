namespace TypingRealm.Authentication
{
    public sealed record Auth0AuthenticationConfiguration : AuthenticationProviderConfiguration
    {
        public Auth0AuthenticationConfiguration() : base(
            Audience: "https://api.typingrealm.com",
            Issuer: "https://typingrealm.us.auth0.com/",
            TokenEndpoint: "https://typingrealm.us.auth0.com/oauth/token",
            AuthorizationEndpoint: "https://typingrealm.us.auth0.com/authorize",
            PkceClientId: "usmQTpTvmVrxC4QtYMYj6R7aIa6Ambck",
            ServiceClientId: "QEYu5cQ7etxQyUARibsLFJwh5HBNOcjv",
            ServiceClientSecret: "uFj6pnafZFYZbabfub_5fdSZC2aQ-Sgw4VP7jsEWgWZQjv4OVMvkFqk25BBpC1Me")
        { }
    }
}
