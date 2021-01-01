namespace TypingRealm.Authentication
{
    public static class Auth0AuthenticationConfiguration
    {
        public static readonly string Audience = "https://api.typingrealm.com";
        public static readonly string Issuer = "https://typingrealm.us.auth0.com/";
        public static readonly string TokenEndpoint = $"{Issuer}oauth/token";
        public static readonly string AuthorizationEndpoint = $"{Issuer}authorize";

        public static readonly string PkceClientId = "usmQTpTvmVrxC4QtYMYj6R7aIa6Ambck";
        public static readonly string ServiceClientId = "QEYu5cQ7etxQyUARibsLFJwh5HBNOcjv";
        public static readonly string ServiceClientSecret = "uFj6pnafZFYZbabfub_5fdSZC2aQ-Sgw4VP7jsEWgWZQjv4OVMvkFqk25BBpC1Me";
    }
}
