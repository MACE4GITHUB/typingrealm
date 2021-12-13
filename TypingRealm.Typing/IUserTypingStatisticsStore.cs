using System.Threading.Tasks;

namespace TypingRealm.Typing
{
    /// <summary>
    /// Cache for user typing statistics.
    /// </summary>
    public interface IUserTypingStatisticsStore
    {
        ValueTask<UserTypingStatistics?> GetUserTypingStatisticsAsync(string userId, string language);
        ValueTask SaveAsync(string userId, UserTypingStatistics userTypingStatistics, string language);
    }
}
