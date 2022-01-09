namespace TypingRealm.DeploymentHelper
{
    public static class HardcodedData
    {
        public static DeploymentData Generate()
        {
            // TODO: Deserialize json.

            return new DeploymentData(new Service[]
            {
                new Service(1, "identityserver", DatabaseType.None, CacheType.None, ".", "TypingRealm.IdentityServer.Host/Dockerfile", 30000, false),

                new Service(10, "texts", DatabaseType.None, CacheType.Redis, ".", "TypingRealm.Texts.Api/Dockerfile", 30401, false),
                new Service(11, "library", DatabaseType.Postgres, CacheType.None, ".", "TypingRealm.Library.Api/Dockerfile", 30402, false),

                new Service(20, "profiles", DatabaseType.None, CacheType.None, ".", "TypingRealm.Profiles.Api/Dockerfile", 30103, true),
                new Service(30, "data", DatabaseType.Postgres, CacheType.Redis, ".", "TypingRealm.Data.Api/Dockerfile", 30400, true)
            }, new Service[]
            {
                // TODO: Move out or mark web-ui so that URL env variable is not created for it.
                new Service(40, "web-ui", DatabaseType.None, CacheType.None, "./typingrealm-web", "Dockerfile", 8080, true)
                {
                    // Should be specified for WEB services.
                    Envs = new[] { "prod", "strict-prod", "dev" }
                }
            });
        }
    }
}
