using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.DeploymentHelper.Data;

public sealed class Environment
{
    public const string Prod = "prod";
    public const string StrictProd = "strict-prod";
    public const string Dev = "dev";
    public const string Local = "local";
    public const string Debug = "debug";
    public static readonly IEnumerable<string> AllowedEnvironments = new[]
    {
        Prod, StrictProd, Dev, Local, Debug
    };

    public Environment(string value)
    {
        if (!AllowedEnvironments.Contains(value))
            throw new ArgumentException("Environment value is not supported.", nameof(value));

        Value = value;
        PortPrefix = "999";

        if (value == Prod || value == StrictProd)
        {
            EnvironmentPrefix = string.Empty;
            EnvironmentFileName = $"{Constants.EnvironmentFilesFolderWithSlash}.env.{Prod}";
            VolumeFolderName = Prod;

            if (value == Prod)
                PortPrefix = "1";
            if (value == StrictProd)
                PortPrefix = "2";

            return;
        }

        EnvironmentPrefix = $"{value}-";
        EnvironmentFileName = $"{Constants.EnvironmentFilesFolderWithSlash}.env.{value}";
        VolumeFolderName = value;

        if (value == Dev)
            PortPrefix = "3";
        if (value == Local)
            PortPrefix = "4";
        if (value == Debug)
            PortPrefix = "5";

        if (PortPrefix == "999")
            throw new InvalidOperationException("Port prefix has not been set to a valid value.");
    }

    public static Environment DevEnvironment => new Environment(Dev);

    public string EnvironmentPrefix { get; }
    public string Value { get; }
    public string EnvironmentFileName { get; }

    public bool OnlyMainNetwork => Value == Debug;

    // Should have single character.
    public string PortPrefix { get; }
    public string VolumeFolderName { get; }
    public bool HideInfrastructurePorts => Value == StrictProd || Value == Local;
    public bool RestartUnlessStopped => Value != Debug;
    public bool DeployInfrastructure => Value != Debug;
    public bool IsDevelopmentEnv => Value != Prod && Value != StrictProd;
    public bool GenerateEnvFiles => Value != StrictProd;
    public bool IsDebug => Value == Debug;
    public bool IsLocal => Value == Local;
}
