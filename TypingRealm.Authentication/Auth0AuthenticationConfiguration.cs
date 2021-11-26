namespace TypingRealm.Authentication
{
    public sealed record Auth0AuthenticationConfiguration : AuthenticationProviderConfiguration
    {
        private const string Auth0Issuer = "https://typingrealm.us.auth0.com/";

        public Auth0AuthenticationConfiguration() : base(
            Audience: "https://api.typingrealm.com",
            Issuer: Auth0Issuer,
            TokenEndpoint: $"{Auth0Issuer}oauth/token",
            AuthorizationEndpoint: $"{Auth0Issuer}authorize",
            PkceClientId: "usmQTpTvmVrxC4QtYMYj6R7aIa6Ambck",
            ServiceClientId: "QEYu5cQ7etxQyUARibsLFJwh5HBNOcjv",
            ServiceClientSecret: "uFj6pnafZFYZbabfub_5fdSZC2aQ-Sgw4VP7jsEWgWZQjv4OVMvkFqk25BBpC1Me")
        { }
    }
}
