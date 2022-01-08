using System;
using System.IO;
using TypingRealm.DeploymentHelper;
using Environment = TypingRealm.DeploymentHelper.Environment;

Console.WriteLine("Folder:");
var folder = Console.ReadLine();
if (folder == null)
    throw new InvalidOperationException("Folder is not specified.");

if (!Directory.Exists(folder))
    Directory.CreateDirectory(folder);

if (!Directory.Exists(Path.Combine(folder, "deployment")))
    Directory.CreateDirectory(Path.Combine(folder, "deployment"));

if (!Directory.Exists(Path.Combine(folder, "reverse-proxy")))
    Directory.CreateDirectory(Path.Combine(folder, "reverse-proxy"));

var environments = new[] { "strict-prod", "prod", "dev", "local", "debug" };

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

foreach (var profile in new[] { "prod", "host", "local" })
{
    File.WriteAllText(
        Path.Combine(folder, "reverse-proxy", $"Caddyfile.{profile}"),
        new CaddyfileGenerator().GenerateCaddyfile(
            HardcodedData.Generate(),
            profile));
}
