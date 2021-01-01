﻿namespace TypingRealm.Authentication
{
    public static class IdentityServerAuthenticationConfiguration
    {
        public static readonly string Audience = "https://localhost:30000/resources";
        public static readonly string Issuer = "https://localhost:30000"; // This should not have slash at the end because identity server by default uses this address as issuer for generating access tokens.
        public static readonly string TokenEndpoint = $"{Issuer}/connect/token";
        public static readonly string AuthorizationEndpoint = $"{Issuer}/connect/authorize";

        public static readonly string PkceClientId = "webapp";
        public static readonly string ServiceClientId = "service";
        public static readonly string ServiceClientSecret = "secret";
    }
}
