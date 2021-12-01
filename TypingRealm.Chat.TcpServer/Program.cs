using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TypingRealm.Hosting.Service;

namespace TypingRealm.Chat.TcpServer
{
    public static class Program
    {
        private const int Port = 40010;

        public static async Task Main()
        {
            using var host = HostFactory.CreateTcpHostBuilder(Port, builder =>
            {
                builder.AddChat();
            }).Build();

            await host.RunAsync().ConfigureAwait(false);
        }
    }
}
