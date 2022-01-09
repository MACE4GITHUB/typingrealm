using System;
using System.Linq;
using System.Text;

namespace TypingRealm.DeploymentHelper.Caddy;

public sealed class CaddyfileGenerator
{
    public string GenerateCaddyfile(DeploymentData data, CaddyProfile profile)
    {
        var sb = new StringBuilder();
        if (profile.SpecifyEmail)
        {
            sb.AppendLine("{");
            sb.AppendLine("    email typingrealm@gmail.com");
            sb.AppendLine("}");
        }

        void GenerateSection(string prefix, CaddyProfile caddyProfile)
        {
            if (sb == null)
                throw new InvalidOperationException("StringBuilder shouldn't be null.");

            var domainPrefix = string.IsNullOrEmpty(prefix) ? string.Empty : $"{prefix}.";
            var servicePrefix = string.IsNullOrEmpty(prefix) ? string.Empty : $"{prefix}-";

            sb.AppendLine();
            sb.AppendLine($"{domainPrefix}{caddyProfile.Domain} {{");
            sb.AppendLine($"    reverse_proxy {servicePrefix}{caddyProfile.WebUiAddress}");
            sb.AppendLine("}");

            sb.AppendLine();
            sb.AppendLine($"{domainPrefix}api.{caddyProfile.Domain} {{");

            foreach (var service in data.Services
                .Where(s => s.ServiceName != "web-ui" && (s.AddToReverseProxyInProduction || !caddyProfile.IsStrictProd))
                .OrderBy(service => service.ServiceName))
            {
                sb.AppendLine($"    handle_path /{service.ServiceName}/* {{");
                sb.AppendLine($"        reverse_proxy {servicePrefix}{caddyProfile.GetReverseProxyAddress(service)}");
                sb.AppendLine("    }");
                sb.AppendLine();
            }

            sb.AppendLine("    respond 404");
            sb.AppendLine("}");
        }

        GenerateSection(string.Empty, profile);

        if (profile.Value == "host")
        {
            GenerateSection("dev", profile);
            GenerateSection(string.Empty, new CaddyProfile("local"));
        }

        return sb.ToString();
    }
}
