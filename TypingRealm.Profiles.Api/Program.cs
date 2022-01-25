using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Profiles.Api.Controllers;
using TypingRealm.Profiles.Infrastructure;

[assembly: ApiController]
namespace TypingRealm.Profiles.Api;

public static class Program
{
    public static async Task Main()
    {
        var builder = HostFactory.CreateWebApiApplicationBuilder(typeof(CharactersController).Assembly);
        builder.Services.RegisterProfilesApi();

        var app = builder.Build();
        await app.RunAsync();
    }
}
