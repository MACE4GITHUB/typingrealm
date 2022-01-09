using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.DeploymentHelper.Caddy;

public sealed class CaddyProfile
{
    public CaddyProfile(string value)
    {
        if (value != "prod" && value != "host" && value != "local")
            throw new ArgumentException("CaddyProfile value is not correct.", nameof(value));

        Value = value;
    }

    public static IEnumerable<CaddyProfile> GetAllProfiles()
        => new[] { "prod", "host", "local" }.Select(x => new CaddyProfile(x)).ToList();

    public string Value { get; }

    public bool IsProd => Value == "prod";
    public bool SpecifyEmail => Value != "local";
    public string Domain
    {
        get
        {
            if (Value == "local")
                return "localhost";

            return "typingrealm.com";
        }
    }

    public string WebUiAddress
    {
        get
        {
            if (Value == "local")
                return "host.docker.internal:4200";

            return "typingrealm-web-ui:80";
        }
    }

    public string GetReverseProxyAddress(Service service)
    {
        if (Value == "local")
            return $"local-typingrealm-{service.ServiceName}:80";

        return $"typingrealm-{service.ServiceName}:80";
    }
}
