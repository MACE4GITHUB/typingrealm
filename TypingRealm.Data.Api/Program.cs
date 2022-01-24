using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        builder.Services.RegisterDataApi();

        await builder.Build()
            .RunAsync();
    }
}
