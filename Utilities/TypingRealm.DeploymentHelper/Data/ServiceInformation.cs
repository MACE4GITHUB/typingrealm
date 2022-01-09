using System.Collections.Generic;

namespace TypingRealm.DeploymentHelper.Data;

public sealed record ServiceInformation(
    string ImageName, string ContainerName,
    IEnumerable<string> Networks,
    BuildConfiguration? BuildConfiguration,
    string MemLimit, string MemReservation,
    IEnumerable<string> EnvFiles,
    IEnumerable<string> Ports,
    IEnumerable<string> Volumes);
