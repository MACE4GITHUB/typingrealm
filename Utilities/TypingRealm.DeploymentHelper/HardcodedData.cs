using TypingRealm.DeploymentHelper.Data;

namespace TypingRealm.DeploymentHelper;

public static class HardcodedData
{
    public static DeploymentData Generate()
    {
        // TODO: Deserialize json.

        return new DeploymentData(new Service[]
        {
            new Service(1, "identityserver", DatabaseType.None, CacheType.None, ".", "TypingRealm.IdentityServer.Host", 30000, false),

            new Service(10, "texts", DatabaseType.None, CacheType.Redis, ".", "TypingRealm.Texts.Api", 30401, false),
            new Service(11, "library", DatabaseType.Postgres, CacheType.None, ".", "TypingRealm.Library.Api", 30402, false),

            new Service(20, "profiles", DatabaseType.None, CacheType.None, ".", "TypingRealm.Profiles.Api", 30103, true),
            new Service(30, "data", DatabaseType.Postgres, CacheType.Redis, ".", "TypingRealm.Data.Api", 30400, true)
        }, new Service[]
        {
            // TODO: Move out or mark web-ui so that URL env variable is not created for it.
            // HACK: Question mark is a hack for now.
            new Service(40, Constants.WebUiServiceName, DatabaseType.None, CacheType.None, "./typingrealm-web", "?", 8080, true)
            {
                // Should be specified for WEB services.
                Envs = new[] { "prod", "strict-prod", "dev" }
            }
        });
    }
}
