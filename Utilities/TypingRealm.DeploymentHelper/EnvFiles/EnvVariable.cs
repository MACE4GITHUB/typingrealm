namespace TypingRealm.DeploymentHelper.EnvFiles;

public sealed record EnvVariable(string Name, string Value)
{
    public override string ToString() => $"{Name}={Value}";
}
