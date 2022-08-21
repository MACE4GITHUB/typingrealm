using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.Hosting;

public static class HostFactory
{
    private static readonly string[] _corsAllowedOrigins = new[]
    {
        "https://typingrealm.com",

        // TODO: Disallow all origins below for production.
        "http://localhost:8080",
        "http://localhost:30080",
        "https://localhost",
        "https://dev.typingrealm.com"
    };

    public static WebApplicationBuilder CreateWebApplicationBuilder()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddHttpClient();
        builder.Services.AddControllers();
        builder.Services.AddTransient<IStartupFilter, WebApiStartupFilter>();
        builder.Services.AddCors(options => options.AddPolicy(
            WebApiStartupFilter.CorsPolicyName,
            builder => builder
                .WithOrigins(_corsAllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));
        builder.Services.AddSingleton<Counter>();

        return builder;
    }
}
