using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.DeploymentHelper.Caddy;

public sealed class CaddyProfile
{
    public const string ProdValue = "prod";
    public const string HostValue = "host";
    public const string LocalValue = "local";

    public CaddyProfile(string value)
    {
        if (value != ProdValue && value != HostValue && value != LocalValue)
            throw new ArgumentException("CaddyProfile value is not correct.", nameof(value));

        Value = value;
    }

    public static IEnumerable<CaddyProfile> GetAllProfiles()
        => new[] { ProdValue, HostValue, LocalValue }.Select(x => new CaddyProfile(x)).ToList();

    public string Value { get; }

    public bool IsProd => Value == ProdValue;
    public bool SpecifyEmail => Value != LocalValue;
    public string Domain
    {
        get
        {
            if (Value == LocalValue)
                return Constants.LocalDomain;

            return Constants.Domain;
        }
    }

    public string WebUiAddress
    {
        get
        {
            if (Value == LocalValue)
                return Constants.LocalWebUiDockerPath;

            return Constants.WebUiDockerPath;
        }
    }
}
