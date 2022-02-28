using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Messaging.Connecting;

public interface IConnectionGroupProvider
{
    ValueTask<IEnumerable<string>> GetGroupsAsync(IConnection connection, CancellationToken cancellationToken);
}
