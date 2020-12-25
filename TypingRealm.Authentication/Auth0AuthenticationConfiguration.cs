namespace TypingRealm.Authentication
{
    public static class Auth0AuthenticationConfiguration
    {
        public static readonly string Audience = "https://api.typingrealm.com";
        public static readonly string Issuer = "https://typingrealm.us.auth0.com/";
        public static readonly string TokenEndpoint = $"{Issuer}oauth/token";
        public static readonly string AuthorizationEndpoint = $"{Issuer}authorize";
    }

    public static class LocalAuthenticationConfiguration
    {
        public static readonly string Audience = "https://api.typingrealm.com";
        public static readonly string Issuer = "https://local-authority"; // This needs to be without slash at the end - or else it will try to query uri.
        public static readonly string TokenEndpoint = $"http://localhost:30103/api/local-token?sub=ivan";

        // TODO: Implement local auth code flow on this endpoint.
        public static readonly string AuthorizationEndpoint = $"http://localhost:30103/api/local-token-auth";
    }
}
