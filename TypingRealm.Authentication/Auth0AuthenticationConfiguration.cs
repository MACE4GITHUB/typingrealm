namespace TypingRealm.Authentication
{
    public static class Auth0AuthenticationConfiguration
    {
        public static readonly string Audience = "https://api.typingrealm.com";
        public static readonly string Issuer = "https://typingrealm.us.auth0.com/";
        public static readonly string TokenEndpoint = $"{Issuer}oauth/token";
        public static readonly string AuthorizationEndpoint = $"{Issuer}authorize";
    }
}
