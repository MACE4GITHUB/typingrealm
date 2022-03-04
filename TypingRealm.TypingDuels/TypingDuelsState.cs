using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.TypingDuels;

public sealed class TypingDuelsState
{
    private readonly object _lock = new object();
    private readonly Dictionary<string, ConcurrentDictionary<string, int>> _clientProgresses = new();

    public void UpdateProgress(string typingSessionId, string clientId, int progress)
    {
        var progresses = GetProgressesForTypingSession(typingSessionId);
        progresses.AddOrUpdate(clientId, progress, (a, b) => progress);
    }

    public IEnumerable<KeyValuePair<string, int>> GetProgressesForSession(string typingSessionId)
    {
        if (!_clientProgresses.ContainsKey(typingSessionId))
            return Enumerable.Empty<KeyValuePair<string, int>>();

        return _clientProgresses[typingSessionId];
    }

    private ConcurrentDictionary<string, int> GetProgressesForTypingSession(string typingSessionId)
    {
        if (_clientProgresses.ContainsKey(typingSessionId))
            return _clientProgresses[typingSessionId];

        lock (_lock)
        {
            if (_clientProgresses.ContainsKey(typingSessionId))
                return _clientProgresses[typingSessionId];

            _clientProgresses.Add(typingSessionId, new());
        }

        return _clientProgresses[typingSessionId];
    }
}
