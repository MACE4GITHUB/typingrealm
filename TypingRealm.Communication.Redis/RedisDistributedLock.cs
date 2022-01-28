using System;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TypingRealm.Communication.Redis;

public sealed class RedisDistributedLock : ILock
{
    private readonly IDatabaseAsync _database;
    private readonly string _lockName;
    private readonly TimeSpan _expirationTime;
    private string? _lockValue;

    public RedisDistributedLock(IDatabaseAsync database, string lockName, TimeSpan expirationTime)
    {
        _database = database;
        _lockName = lockName;
        _expirationTime = expirationTime;
    }

    public async ValueTask ReleaseAsync(CancellationToken cancellationToken)
    {
        if (_lockValue == null)
            throw new InvalidOperationException("Lock has not been acquired yet.");

        var value = await _database.StringGetAsync(GetRedisKey())
            .ConfigureAwait(false);

        if (!value.HasValue)
            return; // Value has expired or was released by another thread.

        if (value != _lockValue)
            throw new InvalidOperationException("Cannot release lock as another thread has acquired it.");

        await _database.KeyDeleteAsync(GetRedisKey())
            .ConfigureAwait(false);
    }

    public async ValueTask WaitAsync(CancellationToken cancellationToken)
    {
        if (_lockValue != null)
            throw new InvalidOperationException("Lock already acquired.");

        _lockValue = Guid.NewGuid().ToString();

        // Simple spinner implementation of lock mechanism.
        while (true)
        {
            var result = await _database.StringSetAsync(
                GetRedisKey(),
                _lockValue,
                when: When.NotExists,
                expiry: _expirationTime).ConfigureAwait(false);

            if (result)
                return;

            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
        }
    }

    private RedisKey GetRedisKey()
    {
        return new RedisKey($"lock_{_lockName}");
    }
}
