using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TypingRealm.Hosting.Service;

namespace TypingRealm.RopeWar.TcpServer;

public static class Program
{
    private const int Port = 30112;

    public static async Task Main()
    {
        using var host = HostFactory.CreateTcpHostBuilder(Port, builder =>
        {
            builder.AddRopeWar();
        }).Build();

        await host.RunAsync().ConfigureAwait(false);
    }
}
