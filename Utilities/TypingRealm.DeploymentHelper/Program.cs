using System.IO;
using TypingRealm.DeploymentHelper;

var folder = @"test";
if (!Directory.Exists(folder))
    Directory.CreateDirectory(folder);

var environments = new[] { "strict-prod", "prod", "dev", "local", "debug" };

foreach (var env in environments)
{
    var compose = new DockerComposeGenerator().Generate(
        HardcodedData.Generate(),
        new Environment(env));

    File.WriteAllText(Path.Combine(folder, $"docker-compose.{env}.yml"), compose);

    var envFiles = new EnvFileGenerator().GenerateEnvFiles(
        HardcodedData.Generate(),
        new Environment(env));

    foreach (var file in envFiles)
    {
        File.WriteAllText(Path.Combine(folder, file.Name), file.Data);
    }
}
