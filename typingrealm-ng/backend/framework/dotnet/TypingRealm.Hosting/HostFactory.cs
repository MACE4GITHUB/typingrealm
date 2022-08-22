using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Hosting;

public sealed record ServiceConfiguration(
    string[] CorsOrigins,
    string DbConnectionString,
    string CacheConnectionString);

public static class HostFactory
{
    public static WebApplicationBuilder CreateWebApplicationBuilder()
    {
        var builder = WebApplication.CreateBuilder();

        // TODO: Move to a separate component.
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

        builder.Services.AddHttpClient();
        builder.Services.AddControllers();
        builder.Services.AddTransient<IStartupFilter, WebApiStartupFilter>();
        builder.Services.AddCors(options => options.AddPolicy(
            WebApiStartupFilter.CorsPolicyName,
            builder => builder
                .WithOrigins(serviceConfiguration!.CorsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));
        builder.Services.AddSingleton<Counter>();

        return builder;
    }
}
