namespace TypingRealm.Authentication
{
    public sealed record LocalAuthenticationConfiguration : AuthenticationProviderConfiguration
    {
        public LocalAuthenticationConfiguration() : base(
            Audience: "local-audience",
            Issuer: "https://local-authority", // This needs to be without slash at the end - or it will try to query uri.
            TokenEndpoint: "http://localhost:30103/api/local-auth/token",
            AuthorizationEndpoint: "http://localhost:30103/api/local-auth/authorize", // TODO: Implement local auth code flow on this endpoint.
            PkceClientId: "local-webapp",
            ServiceClientId: "local-service",
            ServiceClientSecret: "local-secret")
        { }
    }

}
