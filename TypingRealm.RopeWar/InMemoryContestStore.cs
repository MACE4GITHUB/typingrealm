using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.RopeWar;

public sealed class InMemoryContestStore : IContestStore
{
    private readonly object _lock = new object();
    private readonly Dictionary<string, Contest.Data> _cache
        = new Dictionary<string, Contest.Data>();

    public Contest? Find(string contestId)
    {
        lock (_lock)
        {
            if (_cache.ContainsKey(contestId))
                return Contest.FromData(_cache[contestId]);

            return null;
        }
    }

    public Contest? FindActiveByContestantId(string contestantId)
    {
        lock (_lock)
        {
            var data = _cache.Values.FirstOrDefault(
                c => !c.HasEnded && c.Contestants.Values.Any(
                    x => x.Contains(contestantId)));

            if (data == null)
                return null;

            return Contest.FromData(data);
        }
    }

    public void Save(Contest contest)
    {
        lock (_lock)
        {
            var data = contest.GetData();

            if (!_cache.ContainsKey(data.ContestId))
                _cache.Add(data.ContestId, data);

            _cache[data.ContestId] = data;
        }
    }
}
