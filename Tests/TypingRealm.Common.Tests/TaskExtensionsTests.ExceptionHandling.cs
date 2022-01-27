using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Tests;

public sealed class TestSynchronizationContext : SynchronizationContext
{
    public bool IsPostCalled { get; private set; }

    public override void Post(SendOrPostCallback d, object? state)
    {
        IsPostCalled = true;
        base.Post(d, state);
    }
}

public partial class TaskExtensionsTests : TestsBase
{
    [Fact]
    public async Task HandleException_ShouldHandleException()
    {
        var thrown = new TaskCanceledException();
        Exception? exception = null;

        await Task.Run(() => throw thrown)
            .HandleExceptionAsync<TaskCanceledException>(e => exception = e);

        Assert.Equal(thrown, exception);
    }

    [Fact]
    public async Task HandleException_ShouldHandleDerivedException()
    {
        var thrown = new TaskCanceledException();
        Exception? exception = null;

        await Task.Run(() => throw thrown)
            .HandleExceptionAsync<OperationCanceledException>(e => exception = e);

        Assert.Equal(thrown, exception);
    }

    [Fact]
    public async Task HandleException_ShouldNotHandleAnotherException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Task.Run(() => throw new InvalidOperationException())
                .HandleExceptionAsync<OperationCanceledException>(e => { }));
    }

    [Fact]
    public async Task HandleException_ShouldNotHandleWhenNoException()
    {
        var isHandled = false;
        await Task.Run(() => { }).HandleExceptionAsync<OperationCanceledException>(e => isHandled = true);

        Assert.False(isHandled);
    }

    [Fact]
    public async Task HandleCancellation_ShouldHandleCancellation()
    {
        Exception? exception = null;

        await Task.Run(async () =>
        {
            Cts.Cancel();
            await Task.Delay(Timeout.Infinite, Cts.Token);
        }).HandleCancellationAsync(e => exception = e);

        Assert.IsAssignableFrom<OperationCanceledException>(exception);
    }

    [Fact]
    public async Task HandleCancellation_ShouldHandleCancellationException()
    {
        var thrown = new OperationCanceledException();
        Exception? exception = null;

        await Task.Run(() => throw thrown)
            .HandleCancellationAsync(e => exception = e);

        Assert.Equal(thrown, exception);
    }

    [Fact]
    public async Task HandleCancellation_ShouldHandleDerivedCancellationException()
    {
        var thrown = new TaskCanceledException();
        Exception? exception = null;

        await Task.Run(() => throw thrown)
            .HandleCancellationAsync(e => exception = e);

        Assert.Equal(thrown, exception);
    }

    [Fact]
    public async Task HandleCancellation_ShouldNotHandleAnotherException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Task.Run(() => throw new InvalidOperationException())
                .HandleExceptionAsync<OperationCanceledException>(e => { }));
    }

    [Fact]
    public async Task HandleCancellation_ShouldNotHandleWhenNoException()
    {
        var isHandled = false;
        await Task.Run(() => { }).HandleExceptionAsync<OperationCanceledException>(e => isHandled = true);

        Assert.False(isHandled);
    }

    [Fact]
    public async Task SwallowCancellation_ShouldSwallowCancellation()
    {
        await Task.Run(async () =>
        {
            var task = Task.Delay(Timeout.Infinite, Cts.Token);

            await Wait();
            Cts.Cancel();

            await task;
        }).SwallowCancellationAsync();
    }

    [Fact]
    public async Task SwallowCancellation_ShouldSwallowCancellationException()
    {
        await Task.Run(() => throw new OperationCanceledException())
            .SwallowCancellationAsync();
    }

    [Fact]
    public async Task SwallowCancellation_ShouldSwallowDerivedCancellationException()
    {
        await Task.Run(() => throw new TaskCanceledException())
            .SwallowCancellationAsync();
    }

    [Fact]
    public async Task SwallowCancellation_ShouldNotSwallowAnotherException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Task.Run(() => throw new InvalidOperationException())
                .SwallowCancellationAsync());
    }

    [Fact]
    public async Task HandleException_ShouldNotCaptureContextByDefault()
    {
        var context = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        await Task.Run(() => throw new InvalidOperationException())
            .HandleExceptionAsync<InvalidOperationException>(exception => { })
            .ConfigureAwait(false);

        Assert.False(context.IsPostCalled);
    }

    [Fact]
    public async Task HandleException_ShouldCaptureContextWhenSpecified()
    {
        var context = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        await Task.Run(() => throw new InvalidOperationException())
            .HandleExceptionAsync<InvalidOperationException>(exception => { }, true)
            .ConfigureAwait(false);

        Assert.True(context.IsPostCalled);
    }

    [Fact]
    public async Task HandleCancellation_ShouldNotCaptureContextByDefault()
    {
        var context = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        await Task.Run(() => throw new OperationCanceledException())
            .HandleCancellationAsync(exception => { })
            .ConfigureAwait(false);

        Assert.False(context.IsPostCalled);
    }

    [Fact]
    public async Task HandleCancellation_ShouldCaptureContextWhenSpecified()
    {
        var context = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        await Task.Run(() => throw new OperationCanceledException())
            .HandleCancellationAsync(exception => { }, true)
            .ConfigureAwait(false);

        Assert.True(context.IsPostCalled);
    }

    [Fact]
    public async Task SwallowCancellation_ShouldNotCaptureContext()
    {
        var context = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        await Task.Run(() => throw new OperationCanceledException())
            .SwallowCancellationAsync()
            .ConfigureAwait(false);

        Assert.False(context.IsPostCalled);
    }
}
