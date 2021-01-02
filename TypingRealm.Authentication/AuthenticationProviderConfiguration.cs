namespace TypingRealm.Authentication
{
    public abstract record AuthenticationProviderConfiguration(
        string Audience,
        string Issuer,
        string TokenEndpoint,
        string AuthorizationEndpoint,
        string PkceClientId,
        string ServiceClientId,
        string ServiceClientSecret);
}
