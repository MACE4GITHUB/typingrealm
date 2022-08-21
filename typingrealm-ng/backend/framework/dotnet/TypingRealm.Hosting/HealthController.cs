using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace TypingRealm.Hosting;

public sealed class Counter
{
    private int _count;

    public int Next() => Interlocked.Increment(ref _count);
}

public sealed record HealthResponse(int Count);

public sealed class HealthController : ControllerBase
{
    private readonly Counter _counter;

    public HealthController(Counter counter)
    {
        _counter = counter;
    }

    [HttpGet]
    [Route("health")]
    public HealthResponse GetHealth()
    {
        return new HealthResponse(_counter.Next());
    }
}

