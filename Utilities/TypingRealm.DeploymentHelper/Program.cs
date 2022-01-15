using System;
using System.IO;
using TypingRealm.DeploymentHelper;
using TypingRealm.DeploymentHelper.Caddy;
using TypingRealm.DeploymentHelper.Compose;
using TypingRealm.DeploymentHelper.DotNetGeneration;
using TypingRealm.DeploymentHelper.EnvFiles;
using Environment = TypingRealm.DeploymentHelper.Data.Environment;

Console.WriteLine("Folder:");
var folder = Console.ReadLine();
if (string.IsNullOrWhiteSpace(folder))
    folder = @"d:\projects\typingrealm";

if (!Directory.Exists(folder))
    Directory.CreateDirectory(folder);

if (!Directory.Exists(Path.Combine(folder, "deployment")))
    Directory.CreateDirectory(Path.Combine(folder, "deployment"));

if (!Directory.Exists(Path.Combine(folder, "reverse-proxy")))
    Directory.CreateDirectory(Path.Combine(folder, "reverse-proxy"));

var environments = new[] { "strict-prod", "prod", "dev", "local", "debug" };

File.WriteAllText(Path.Combine(folder, "deployment", ".env"), string.Empty);
foreach (var env in environments)
{
    var compose = new DockerComposeGenerator().Generate(
        HardcodedData.Generate(),
        new Environment(env));

    if (env == "debug")
    {
        // Debug profile is the default one.
        File.WriteAllText(Path.Combine(folder, $"docker-compose.yml"), compose);
    }
    else
    {
        File.WriteAllText(Path.Combine(folder, $"docker-compose.{env}.yml"), compose);
    }

    var envFiles = new EnvFileGenerator().GenerateEnvFiles(
        HardcodedData.Generate(),
        new Environment(env));

    foreach (var file in envFiles)
    {
        File.WriteAllText(Path.Combine(folder, "deployment", file.Name), file.Data);
    }
}

foreach (var profile in CaddyProfile.GetAllProfiles())
{
    File.WriteAllText(
        Path.Combine(folder, "reverse-proxy", $"Caddyfile.{profile.Value}"),
        new CaddyfileGenerator().GenerateCaddyfile(
            HardcodedData.Generate(),
            profile));
}

new ServiceGenerator().GenerateServices(folder, HardcodedData.Generate().Services);
