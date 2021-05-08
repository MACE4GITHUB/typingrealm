using System.Threading.Tasks;

namespace TypingRealm.Messaging.Client
{
    public interface IProfileTokenProvider
    {
        // TODO: Pass cancellation token here.
        ValueTask<string> SignInAsync();
    }
}
