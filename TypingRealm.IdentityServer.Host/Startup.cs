using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.IdentityServer.Host;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        _ = services.AddIdentityServer(options =>
        {
            // This can be turned off, if we turn off AudienceValidation everywhere as well.
            options.EmitStaticAudienceClaim = true;
        })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients);

        if (DebugHelpers.IsDevelopment())
        {
            services.AddCors(options => options.AddPolicy(
                "DevelopmentCorsPolicy",
                builder => builder
                    .WithOrigins(GetDevelopmentAllowedOrigins().ToArray())
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));
        }
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseCors("DevelopmentCorsPolicy");
        app.UseEndpoints(e => e.MapControllers());
        app.UseIdentityServer();
    }

    private static IEnumerable<string> GetDevelopmentAllowedOrigins()
    {
        var hosts = new[]
        {
                "127.0.0.1",
                "localhost",
                "typingrealm.com",
                "dev.typingrealm.com"
            };

        var ports = new[]
        {
                "",
                ":4200"
            };

        foreach (var host in hosts)
        {
            foreach (var port in ports)
            {
                yield return $"http://{host}{port}";
                yield return $"https://{host}{port}";
            }
        }
    }
}
