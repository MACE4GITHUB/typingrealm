using System.Collections.Generic;
using Duende.IdentityServer.Models;

namespace TypingRealm.IdentityServer.Host
{
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
                new ApiScope("m2m", "Obsolete scope")
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
                        "service",
                        "m2m"
                    },
                    AccessTokenLifetime = 90
                }
            };
    }
}
