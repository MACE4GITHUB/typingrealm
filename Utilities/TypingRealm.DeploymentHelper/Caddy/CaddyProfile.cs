using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.DeploymentHelper.Caddy;

public sealed class CaddyProfile
{
    public const string ProdValue = "prod";
    public const string HostValue = "host";
    public const string LocalValue = "local";
    public static readonly string[] AllowedValues
        = new[] { ProdValue, HostValue, LocalValue };

    public CaddyProfile(string value)
    {
        if (!AllowedValues.Contains(value))
            throw new ArgumentException("CaddyProfile value is not supported.", nameof(value));

        Value = value;
    }

    public static IEnumerable<CaddyProfile> GetAllProfiles() => AllowedValues
        .Select(x => new CaddyProfile(x))
        .ToList();

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
