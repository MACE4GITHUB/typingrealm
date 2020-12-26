namespace TypingRealm.Authentication
{
    public static class LocalAuthenticationConfiguration
    {
        public static readonly string Audience = "local-audience";
        public static readonly string Issuer = "https://local-authority"; // This needs to be without slash at the end - or else it will try to query uri.
        public static readonly string TokenEndpoint = $"http://localhost:30103/api/local-auth/token";

        // TODO: Implement local auth code flow on this endpoint.
        public static readonly string AuthorizationEndpoint = $"http://localhost:30103/api/local-auth/authorize";
    }
}
