using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Typing.Infrastructure
{
    public sealed class InMemoryUserTypingStatisticsStore : IUserTypingStatisticsStore
    {
        private readonly IDictionary<string, UserTypingStatistics> _userTypingStatistics
            = new Dictionary<string, UserTypingStatistics>();

        public async ValueTask<UserTypingStatistics?> GetUserTypingStatisticsAsync(string userId)
        {
            if (!_userTypingStatistics.ContainsKey(userId))
                return null;

            return _userTypingStatistics[userId];
        }

        public ValueTask SaveAsync(string userId, UserTypingStatistics userTypingStatistics)
        {
            if (_userTypingStatistics.ContainsKey(userId))
            {
                _userTypingStatistics[userId] = userTypingStatistics;
                return default;
            }

            _userTypingStatistics.Add(userId, userTypingStatistics);
            return default;
        }
    }
}
