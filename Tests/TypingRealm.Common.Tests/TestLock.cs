using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Common.Tests;

public sealed class TestLock : ILock
{
    public enum State
    {
        None,
        Started,
        Canceled,
        Completed
    }

    public bool WaitComplete { get; set; }
    public bool ReleaseComplete { get; set; }

    public State WaitState { get; private set; }
    public State ReleaseState { get; private set; }

    public async ValueTask ReleaseAsync(CancellationToken cancellationToken)
    {
        ReleaseState = State.Started;

        while (!ReleaseComplete)
        {
            await Task.Yield();
        }

        ReleaseState = State.Completed;
    }

    public async ValueTask WaitAsync(CancellationToken cancellationToken)
    {
        WaitState = State.Started;

        while (!WaitComplete)
        {
            await Task.Yield();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                WaitState = State.Canceled;
                throw;
            }
        }

        WaitState = State.Completed;
    }
}
