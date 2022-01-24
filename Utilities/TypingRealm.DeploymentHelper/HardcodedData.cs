using TypingRealm.DeploymentHelper.Data;

namespace TypingRealm.DeploymentHelper;

public static class HardcodedData
{
    public static DeploymentData Generate()
    {
        // TODO: Deserialize json.

        return new DeploymentData(new Service[]
        {
            new Service(1, Constants.AuthorityServiceName, DatabaseType.None, CacheType.None, ".", 30000, false),

            new Service(10, "Texts", DatabaseType.None, CacheType.Redis, ".", 30401, false),
            new Service(11, "Library", DatabaseType.Postgres, CacheType.None, ".", 30402, false),
            new Service(12, "Typing", DatabaseType.Postgres, CacheType.Redis, ".", 30403, true),

            new Service(20, "Profiles", DatabaseType.None, CacheType.None, ".", 30103, true),
            new Service(30, "Data", DatabaseType.Postgres, CacheType.Redis, ".", 30400, true)
        }, new Service[]
        {
            // TODO: Move out or mark web-ui so that URL env variable is not created for it.
            // HACK: Question mark is a hack for now.
            new Service(40, Constants.WebUiServiceName, DatabaseType.None, CacheType.None, "./typingrealm-web", 8080, true)
            {
                // Should be specified for WEB services.
                Envs = new[] { "prod", "strict-prod", "dev" }
            }
        });
    }
}
