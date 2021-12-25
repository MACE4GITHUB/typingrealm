using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm
{
    /// <summary>
    /// Lock that allows only one thread to enter at a time.
    /// </summary>
    public interface ILock
    {
        ValueTask WaitAsync(CancellationToken cancellationToken);
        ValueTask ReleaseAsync(CancellationToken cancellationToken);
    }
}
