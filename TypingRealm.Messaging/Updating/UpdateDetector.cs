using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.Messaging.Updating;

public sealed class UpdateDetector : IUpdateDetector
{
    private readonly object _lock = new object();
    private readonly HashSet<string> _marked
        = new HashSet<string>();

    public void MarkForUpdate(IEnumerable<string> groups)
    {
        lock (_lock)
        {
            foreach (var group in groups)
            {
                _marked.Add(group);
            }
        }
    }

    public IEnumerable<string> PopMarked()
    {
        lock (_lock)
        {
            var values = _marked.ToList();
            _marked.Clear();
            return values;
        }
    }
}
