namespace TypingRealm.Authentication
{
    public sealed record IdentityServerAuthenticationConfiguration : AuthenticationProviderConfiguration
    {
        public IdentityServerAuthenticationConfiguration() : base(
            Audience: "https://localhost:30000/resources",
            Issuer: "https://localhost:30000/",
            TokenEndpoint: "https://localhost:30000/connect/token",
            AuthorizationEndpoint: "https://localhost:30000/connect/authorize",
            PkceClientId: "webapp",
            ServiceClientId: "service",
            ServiceClientSecret: "secret")
        { }
    }
}
