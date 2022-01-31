using System.Collections.Generic;
using Duende.IdentityServer.Models;

namespace TypingRealm.IdentityServer.Host;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
                new IdentityResources.OpenId()
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
                new ApiScope("service", "TypingRealm internal service-to-service communication."),
                new ApiScope("diagnostics", "TypingRealm internal diagnostics scope for diagnostics operations."),
                new ApiScope("realtime-auth", "Used to authenticate realtime communication.")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
                new Client
                {
                    ClientId = "service",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes =
                    {
                        "service"
                    },
                    AccessTokenLifetime = 3600
                },
                new Client
                {
                    ClientId = "diagnostics",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("diagnostics".Sha256())
                    },
                    AllowedScopes =
                    {
                        "diagnostics",
                        "service"
                    },
                    AccessTokenLifetime = 60
                },
                new Client
                {
                    // TODO: Instead of pure CC flow use exchange, and send a valid USER token to this endpoint.
                    // Or at least send user ID and save it in the claims.
                    ClientId = "realtime-auth",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes =
                    {
                        "realtime-auth",
                        "service"
                    },
                    AccessTokenLifetime = 20
                }
        };
}
