using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Tests;

public partial class TaskExtensionsTests : TestsBase
{
    [Fact]
    public async Task WithCancellation_ShouldSucceedWhenCompleted()
    {
        var result = Task.Run(async () =>
        {
            await Task.Delay(1);
        }).WithCancellationAsync(Cts.Token);
        await result;
        Cts.Cancel();

        Assert.True(result.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task WithCancellationWithValue_ShouldSucceedWhenCompleted()
    {
        var result = Task.Run(async () =>
        {
            await Task.Delay(1);
            return 10;
        }).WithCancellationAsync(Cts.Token);
        var value = await result;
        Cts.Cancel();

        Assert.Equal(10, value);
        Assert.True(result.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task WithCancellation_ShouldThrowWhenCancellationRequested()
    {
        var task = Task.Run(async () =>
        {
            await Task.Delay(Timeout.Infinite);
        });

        var result = task.WithCancellationAsync(Cts.Token);

        Cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => result);
        Assert.True(result.IsCanceled);
    }

    [Fact]
    public async Task WithCancellationWithValue_ShouldThrowWhenCancellationRequested()
    {
        var task = Task.Run(async () =>
        {
            await Task.Delay(Timeout.Infinite);
            return 10;
        });

        var result = task.WithCancellationAsync(Cts.Token);

        Cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => result);
        Assert.True(result.IsCanceled);
    }

    [Fact]
    public async Task WithCancellation_ShouldAwaitTargetTaskWhenExceptionIsThrown()
    {
        var result = Task.Run(async () =>
        {
            await Task.Delay(10);
            throw new TestException();
        }).WithCancellationAsync(Cts.Token);

        await Assert.ThrowsAsync<TestException>(() => result);
        Assert.True(result.IsFaulted);
    }

    [Fact]
    public async Task WithCancellationWithValue_ShouldAwaitTargetTaskWhenExceptionIsThrown()
    {
        var result = Task.Run<int>(async () =>
        {
            await Task.Delay(10);
            throw new TestException();
        }).WithCancellationAsync(Cts.Token);

        await Assert.ThrowsAsync<TestException>(() => result);
        Assert.True(result.IsFaulted);
    }
}
