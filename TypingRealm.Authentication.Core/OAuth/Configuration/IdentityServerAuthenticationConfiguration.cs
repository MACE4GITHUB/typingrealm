using System;

namespace TypingRealm.Authentication.OAuth.Configuration;

public sealed record IdentityServerAuthenticationConfiguration : AuthenticationProviderConfiguration
{
    private const string LocalhostAuthority = "http://localhost:30000/";
    public IdentityServerAuthenticationConfiguration() : this(GetIdentityServerIssuer()) { }

    /// <summary>
    /// Issuer should have slash at the end.
    /// </summary>
    public IdentityServerAuthenticationConfiguration(string issuer) : base(
        Audience: $"{issuer}resources",
        Issuer: issuer,
        TokenEndpoint: $"{issuer}connect/token",
        AuthorizationEndpoint: $"{issuer}connect/authorize",
        PkceClientId: "webapp",
        ServiceClientId: "service",
        ServiceClientSecret: "secret",
        SuppressErrorOnDiscovery: false)
    {
        // If at any point of time we are going to expose IdentityServer outside
        // (and use it for user tokens), this should be set to true.
        //
        // Currently since we are using it only for in-cluster service-to-service
        // communication, this can be set to false.
        RequireHttpsMetadata = false;
    }

    private static string GetIdentityServerIssuer()
    {
        // HACK: Make sure when proper configuration is implemented we are not getting environment variables here.
        // And also in service discovery.
        var serviceAuthority = Environment.GetEnvironmentVariable("SERVICE_AUTHORITY");

        if (serviceAuthority == null)
            return LocalhostAuthority;

        return serviceAuthority;
    }
}
