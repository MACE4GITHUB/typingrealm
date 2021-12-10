using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TypingRealm.Data.Api.Controllers;
using TypingRealm.Data.Infrastructure;
using TypingRealm.Hosting;

[assembly: ApiController]
namespace TypingRealm.Data.Api;

public static class Program
{
    public static async Task Main()
    {
        var builder = HostFactory.CreateWebApiApplicationBuilder(typeof(LocationsController).Assembly);

        var dataConnectionString = builder.Configuration.GetConnectionString("DataConnection");
        var cacheConnectionString = builder.Configuration.GetConnectionString("CacheConnection");
        var dataCacheConnectionString = builder.Configuration.GetConnectionString("ServiceCacheConnection");
        builder.Services.RegisterDataApi(dataConnectionString, cacheConnectionString, dataCacheConnectionString);

        builder.Services.AddHealthChecks()
            .AddRedis(cacheConnectionString)
            .AddNpgSql(dataConnectionString);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            await OnAppStartup(scope.ServiceProvider);
        }

        await app.RunAsync();
    }

    private class Startup { }
    private static async Task OnAppStartup(IServiceProvider serviceProvider)
    {
        var infrastructureDeploymentService = serviceProvider.GetRequiredService<IInfrastructureDeploymentService>();
        var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();

        await infrastructureDeploymentService.DeployInfrastructureAsync();
        logger.LogInformation("Database is successfully migrated.");
    }
}
