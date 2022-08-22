using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Configuration;

namespace TypingRealm.Hosting;

public static class HostFactory
{
    public static WebApplicationBuilder CreateWebApplicationBuilder()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddTyrConfiguration();
        var configuration = builder.Services.BuildServiceProvider().GetRequiredService<IServiceConfiguration>();

        builder.Services.AddHttpClient();
        builder.Services.AddControllers();
        builder.Services.AddTransient<IStartupFilter, WebApiStartupFilter>();
        builder.Services.AddCors(options => options.AddPolicy(
            WebApiStartupFilter.CorsPolicyName,
            builder => builder
                .WithOrigins(configuration.CorsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));
        builder.Services.AddSingleton<Counter>();

        return builder;
    }
}
