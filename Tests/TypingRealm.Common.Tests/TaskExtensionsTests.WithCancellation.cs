using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Tests
{
    public partial class TaskExtensionsTests
    {
        [Fact]
        public async Task WithCancellation_ShouldSucceedWhenCompleted()
        {
            using var cts = new CancellationTokenSource();

            var result = Task.Run(async () =>
            {
                await Task.Delay(1);
            }).WithCancellationAsync(cts.Token);
            await result;
            cts.Cancel();

            Assert.True(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task WithCancellation_ShouldThrowWhenCancellationRequested()
        {
            var task = Task.Run(async () =>
            {
                await Task.Delay(Timeout.Infinite);
            });

            using var cts = new CancellationTokenSource();
            var result = task.WithCancellationAsync(cts.Token);

            cts.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => result);
            Assert.True(result.IsCanceled);
        }

        [Fact]
        public async Task WithCancellation_ShouldAwaitTargetTaskWhenExceptionIsThrown()
        {
            using var cts = new CancellationTokenSource();

            var result = Task.Run(async () =>
            {
                await Task.Delay(10);
                throw new TestException();
            }).WithCancellationAsync(cts.Token);

            await Assert.ThrowsAsync<TestException>(() => result);
            Assert.True(result.IsFaulted);
        }

        [Fact]
        public async Task WithCancellationWithValue_ShouldSucceedWhenCompleted()
        {
            using var cts = new CancellationTokenSource();

            var result = Task.Run(async () =>
            {
                await Task.Delay(1);
                return 10;
            }).WithCancellationAsync(cts.Token);
            var value = await result;
            cts.Cancel();

            Assert.Equal(10, value);
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task WithCancellationWithValue_ShouldThrowWhenCancellationRequested()
        {
            var task = Task.Run(async () =>
            {
                await Task.Delay(Timeout.Infinite);
                return 10;
            });

            using var cts = new CancellationTokenSource();
            var result = task.WithCancellationAsync(cts.Token);

            cts.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => result);
            Assert.True(result.IsCanceled);
        }

        [Fact]
        public async Task WithCancellationWithValue_ShouldAwaitTargetTaskWhenExceptionIsThrown()
        {
            using var cts = new CancellationTokenSource();

            var result = Task.Run<int>(async () =>
            {
                await Task.Delay(10);
                throw new TestException();
            }).WithCancellationAsync(cts.Token);

            await Assert.ThrowsAsync<TestException>(() => result);
            Assert.True(result.IsFaulted);
        }
    }
}
