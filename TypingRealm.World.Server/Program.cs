using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TypingRealm.Authentication.Service;
using TypingRealm.Hosting.Service;

namespace TypingRealm.World.Server;

public static class Program
{
    public static async Task Main()
    {
        using var host = HostFactory.CreateSignalRHostBuilder(builder =>
        {
            builder.AddWorld();
            builder.Services.AddCharacterAuthentication();
        }).Build();

        await host.RunAsync().ConfigureAwait(false);
    }
}
