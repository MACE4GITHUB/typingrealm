using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Typing.Infrastructure
{
    public sealed class InMemoryUserTypingStatisticsStore : IUserTypingStatisticsStore
    {
        private readonly IDictionary<string, UserTypingStatistics> _userTypingStatistics
            = new Dictionary<string, UserTypingStatistics>();

        public async ValueTask<UserTypingStatistics?> GetUserTypingStatisticsAsync(string userId, string language)
        {
            if (!_userTypingStatistics.ContainsKey(GetKey(userId, language)))
                return null;

            return _userTypingStatistics[GetKey(userId, language)];
        }

        public ValueTask SaveAsync(string userId, UserTypingStatistics userTypingStatistics, string language)
        {
            if (_userTypingStatistics.ContainsKey(GetKey(userId, language)))
            {
                _userTypingStatistics[userId] = userTypingStatistics;
                return default;
            }

            _userTypingStatistics.Add(GetKey(userId, language), userTypingStatistics);
            return default;
        }

        private string GetKey(string userId, string language)
        {
            return $"{userId}_{language}";
        }
    }
}
