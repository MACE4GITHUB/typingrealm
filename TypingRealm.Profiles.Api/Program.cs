using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using TypingRealm.Hosting;
using TypingRealm.Profiles.Api.Controllers;
using TypingRealm.Profiles.Infrastructure;

[assembly: ApiController]
namespace TypingRealm.Profiles.Api
{
    public static class Program
    {
        public static async Task Main()
        {
            using var host = HostFactory.CreateWebApiHostBuilder(typeof(CharactersController).Assembly, services =>
            {
                services.RegisterProfilesApi();
            }).Build();

            await host.RunAsync();
        }
    }
}
