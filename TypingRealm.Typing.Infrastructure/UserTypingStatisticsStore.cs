using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace TypingRealm.Typing.Infrastructure;

public sealed class UserTypingStatisticsStore : IUserTypingStatisticsStore
{
    private readonly IDistributedCache _cache;

    public UserTypingStatisticsStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async ValueTask<UserTypingStatistics?> GetUserTypingStatisticsAsync(string userId, string language, TextGenerationType textGenerationType)
    {
        var stringData = await _cache.GetStringAsync($"typingrealm-data-user-statistics-{userId}_{language}_{textGenerationType}")
            .ConfigureAwait(false);

        if (stringData == null || stringData.Length == 0)
            return null;

        return JsonSerializer.Deserialize<UserTypingStatistics>(stringData);
    }

    public async ValueTask SaveAsync(string userId, UserTypingStatistics userTypingStatistics, string language, TextGenerationType textGenerationType)
    {
        var stringData = JsonSerializer.Serialize(userTypingStatistics);

        await _cache.SetStringAsync($"typingrealm-data-user-statistics-{userId}_{language}_{textGenerationType}", stringData)
            .ConfigureAwait(false);
    }
}
