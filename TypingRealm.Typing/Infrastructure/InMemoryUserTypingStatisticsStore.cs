using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypingRealm.Typing.Infrastructure;

public sealed class InMemoryUserTypingStatisticsStore : IUserTypingStatisticsStore
{
    private readonly IDictionary<string, UserTypingStatistics> _userTypingStatistics
        = new Dictionary<string, UserTypingStatistics>();

    public async ValueTask<UserTypingStatistics?> GetUserTypingStatisticsAsync(string userId, string language, TextGenerationType textGenerationType)
    {
        if (!_userTypingStatistics.ContainsKey(GetKey(userId, language, textGenerationType)))
            return null;

        return _userTypingStatistics[GetKey(userId, language, textGenerationType)];
    }

    public ValueTask SaveAsync(string userId, UserTypingStatistics userTypingStatistics, string language, TextGenerationType textGenerationType)
    {
        if (_userTypingStatistics.ContainsKey(GetKey(userId, language, textGenerationType)))
        {
            _userTypingStatistics[userId] = userTypingStatistics;
            return default;
        }

        _userTypingStatistics.Add(GetKey(userId, language, textGenerationType), userTypingStatistics);
        return default;
    }

    private string GetKey(string userId, string language, TextGenerationType textGenerationType)
    {
        return $"{userId}_{language}_{textGenerationType}";
    }
}
