using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Client
{
    public interface IProfileTokenProvider
    {
        ValueTask<string> SignInAsync(CancellationToken cancellationToken);
    }
}
