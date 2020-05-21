using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace TypingRealm.TcpServer
{
    public static class Program
    {
        public static async Task Main()
        {
            _ = new ServiceCollection()
                .BuildServiceProvider();
        }
    }
}
