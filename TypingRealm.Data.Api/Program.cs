using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        await builder.Build()
            .RunAsync();
    }
}
