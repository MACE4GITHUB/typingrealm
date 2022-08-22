using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Configuration;

public static class RegistrationExtensions
{
    public static IServiceCollection AddTyrConfiguration(this IServiceCollection services)
    {
        var json = File.ReadAllText("../../config.json");
        var matches = Regex.Matches(json, "\"env:[^\"]+\"");
        foreach (var match in matches)
        {
            var matchValue = match.ToString()!.Trim('\"');
            var envValue = Environment.GetEnvironmentVariable(matchValue.Split(':')[1]);

            if (envValue == null)
                throw new InvalidOperationException($"Environment variable {matchValue.Split(':')[1]} is not set.");
            json = json.Replace(matchValue, envValue);
        }

        var serviceConfiguration = JsonSerializer.Deserialize<ServiceConfiguration>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return services.AddSingleton<IServiceConfiguration>(serviceConfiguration!);
    }
}
