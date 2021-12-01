using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TypingRealm.Hosting.Service;

namespace TypingRealm.RopeWar.Server
{
    public static class Program
    {
        public static async Task Main()
        {
            using var host = HostFactory.CreateSignalRHostBuilder(builder =>
            {
                builder.AddRopeWar();
            }).Build();

            await host.RunAsync().ConfigureAwait(false);
        }
    }
}
