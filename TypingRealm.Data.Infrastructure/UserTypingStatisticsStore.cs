using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using TypingRealm.Typing;

namespace TypingRealm.Data.Infrastructure
{
    public sealed class UserTypingStatisticsStore : IUserTypingStatisticsStore
    {
        private readonly ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect(
            new ConfigurationOptions
            {
                EndPoints = { "host.docker.internal:6379" }
            });

        public async ValueTask<UserTypingStatistics?> GetUserTypingStatisticsAsync(string userId)
        {
            var db = _redis.GetDatabase();
            var data = await db.StringGetAsync(new RedisKey($"typingrealm-data-user-statistics-{userId}"))
                .ConfigureAwait(false);

            if (!data.HasValue)
                return null;

            return JsonSerializer.Deserialize<UserTypingStatistics>(data);
        }

        public async ValueTask SaveAsync(string userId, UserTypingStatistics userTypingStatistics)
        {
            var data = JsonSerializer.Serialize(userTypingStatistics);
            var db = _redis.GetDatabase();
            await db.StringSetAsync(new RedisKey($"typingrealm-data-user-statistics-{userId}"), new RedisValue(data))
                .ConfigureAwait(false);
        }
    }
}
