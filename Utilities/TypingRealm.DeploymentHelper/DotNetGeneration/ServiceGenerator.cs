using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using TypingRealm.DeploymentHelper.Data;

namespace TypingRealm.DeploymentHelper.DotNetGeneration;

public sealed class ServiceGenerator
{
    public static readonly string NetVersion = "net6.0";

    public void GenerateServices(string rootFolder, IEnumerable<Service> services)
    {
        var legacyServices = new[] { "data", "profiles" };

        foreach (var service in services.Where(s => !legacyServices.Contains(s.ServiceName)))
        {
            GenerateService(rootFolder, service);
        }

        var lines = services.Where(s => s.RawServiceName != Constants.AuthorityServiceName)
            .OrderBy(s => s.ServiceName)
            .Select(s => @$"{new string(' ', 16)}[""{s.ServiceName}""] = Environment.GetEnvironmentVariable(""{s.ServiceName.ToUpperInvariant()}_URL"") ?? ""http://127.0.0.1:{s.Port}"",")
            .ToList();

        lines[^1] = lines[^1][0..^1];

        var filePath = Path.Combine(rootFolder, $@"{Constants.CommunicationProjectName}/InMemoryServiceClient.cs");
        var serviceClientContent = File.ReadAllText(filePath);
        var contentLines = serviceClientContent.Split('\n');
        var newLines = new List<string>();
        var step = 0;
        foreach (var line in contentLines)
        {
            if (step == 0 && !line.Contains("<string, string> _serviceAddresses"))
            {
                newLines.Add(line);
                continue;
            }

            if (step == 0 && line.Contains("<string, string> _serviceAddresses"))
            {
                newLines.Add(line);
                step = 1;
                continue;
            }

            if (step == 1 && !line.Contains("new Dictionary<string, string>"))
            {
                newLines.Add(line);
                step = 0;
                continue;
            }

            if (step == 1 && line.Contains("new Dictionary<string, string>"))
            {
                newLines.Add(line);
                step = 2;
                continue;
            }

            if (step == 2 && line.Trim().Trim('\r') == "{")
            {
                newLines.Add(line);
                step = 3;
                continue;
            }
            else if (step == 2)
            {
                newLines.Add(line);
                step = 0;
                continue;
            }

            if (step == 3 && line.Trim().Trim('\r') != "};")
                continue;

            if (step == 3 && line.Trim().Trim('\r') == "};")
            {
                newLines.AddRange(lines);
                newLines.Add(line);
                step = -1;
                continue;
            }

            newLines.Add(line);
        }

        File.WriteAllText(filePath, string.Join('\n', newLines));
    }

    private void GenerateService(string rootFolder, Service service)
    {
        if (service.RawServiceName == Constants.AuthorityServiceName)
            return; // Do not change anything for IdentityServer project.

        var serviceFolder = Path.Combine(rootFolder, service.BaseProjectFolder);
        if (!Directory.Exists(serviceFolder))
            Directory.CreateDirectory(serviceFolder);

        static string PrepareFolder(string rootFolder, string projectName)
        {
            var folder = Path.Combine(rootFolder, projectName);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return Path.Combine(rootFolder, projectName, $"{projectName}.csproj");
        }

        // Core project.
        {
            var file = PrepareFolder(rootFolder, service.ServiceProjects.CorePath);

            if (!File.Exists(file))
            {
                CreateProjectFile(file, rootNamespace: service.BaseProjectFolder);
            }
            else
            {
                UpdateProjectFile(file, rootNamespace: service.BaseProjectFolder);
            }
        }

        // Domain project.
        {
            var file = PrepareFolder(rootFolder, service.ServiceProjects.DomainPath);

            var projects = new[]
            {
                service.ServiceProjects.CorePath
            };

            if (!File.Exists(file))
            {
                CreateProjectFile(file, includeProjects: projects);
            }
            else
            {
                UpdateProjectFile(file, includeProjects: projects);
            }
        }

        // API Client project.
        {
            var file = PrepareFolder(rootFolder, service.ServiceProjects.ApiClientPath);

            var projects = new[]
            {
                service.ServiceProjects.CorePath,
                Constants.CommunicationProjectName
            };

            if (!File.Exists(file))
            {
                CreateProjectFile(file, includeProjects: projects);
            }
            else
            {
                UpdateProjectFile(file, includeProjects: projects);
            }
        }

        // Infrastructure project.
        {
            var file = PrepareFolder(rootFolder, service.ServiceProjects.InfrastructurePath);

            var projects = new[]
            {
                service.ServiceProjects.DomainPath
            };

            if (!File.Exists(file))
            {
                CreateProjectFile(file, includeProjects: projects);
            }
            else
            {
                UpdateProjectFile(file, includeProjects: projects);
            }
        }

        // API project.
        {
            var file = PrepareFolder(rootFolder, service.ServiceProjects.ApiPath);

            var projects = new[]
            {
                service.ServiceProjects.InfrastructurePath,
                Constants.HostingProjectName
            };

            if (!File.Exists(file))
            {
                CreateProjectFile(file, includeProjects: projects);
            }
            else
            {
                UpdateProjectFile(file, includeProjects: projects);
            }
        }
    }

    private static void CreateProjectFile(
        string fileName,
        IEnumerable<string>? includeProjects = null,
        string? rootNamespace = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine(@"<Project Sdk=""Microsoft.NET.Sdk"">");
        sb.AppendLine();
        sb.AppendLine("  <PropertyGroup>");
        sb.AppendLine($"    <TargetFramework>{NetVersion}</TargetFramework>");

        if (rootNamespace != null)
            sb.AppendLine($"    <RootNamespace>{rootNamespace}</RootNamespace>");

        sb.AppendLine("  </PropertyGroup>");
        sb.AppendLine();

        if (includeProjects != null)
        {
            sb.AppendLine("  <ItemGroup>");

            foreach (var project in includeProjects.OrderBy(x => x))
            {
                sb.AppendLine(@$"    <ProjectReference Include=""..\{project}\{project}.csproj"" />");
            }

            sb.AppendLine("  </ItemGroup>");
            sb.AppendLine();
        }

        sb.AppendLine("</Project>");

        File.WriteAllText(fileName, sb.ToString());
    }

    private static void UpdateProjectFile(
        string fileName,
        IEnumerable<string>? includeProjects = null,
        string? rootNamespace = null)
    {
        XDocument document;
        using (var stream = File.OpenRead(fileName))
        {
            document = XDocument.Load(stream, LoadOptions.PreserveWhitespace);
            var targetFramework = document.Root!.Element("PropertyGroup")!.Element("TargetFramework")!;
            if (targetFramework.Value != NetVersion)
                targetFramework.Value = NetVersion;

            if (rootNamespace != null)
            {
                var element = document.Root!.Element("PropertyGroup")!.Element("RootNamespace")!;
                if (element.Value != rootNamespace)
                    element.Value = rootNamespace;
            }

            if (includeProjects != null)
            {
                foreach (var project in includeProjects.OrderBy(x => x))
                {
                    var value = $@"..\{project}\{project}.csproj";
                    var element = document.Root?.Elements("ItemGroup").Descendants()
                        .FirstOrDefault(x => x.Name == "ProjectReference" && x.Attribute("Include")?.Value == @$"..\{project}\{project}.csproj");

                    if (element == null)
                    {
                        var group = document.Root?.Elements("ItemGroup").FirstOrDefault(x => x.Descendants().Any(d => d.Attribute("Include")?.Value?.StartsWith(@"..\", System.StringComparison.Ordinal) ?? false));
                        if (group == null)
                            throw new NotSupportedException("Adding a new ItemGroup collection is not supported yet.");

                        var newElement = new XElement("ProjectReference");
                        newElement.SetAttributeValue("Include", value);
                        group.Add(new XText("  "));
                        group.Add(newElement);
                        group.Add(new XText("\n  "));
                    }
                }
            }
        }

        var xws = new XmlWriterSettings { OmitXmlDeclaration = true };
        using var xw = XmlWriter.Create(fileName, xws);
        document.Save(xw);
    }
}
