using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Tests
{
    public class AsyncHelpersTests
    {
        [Fact]
        public async Task WhenAll_ShouldCompleteAsynchronously()
        {
            var isRunning = true;

            var empty = new ValueTask();
            var running = new ValueTask(Task.Run(async () =>
            {
                while (isRunning)
                    await WaitAsync();
            }));

            var result = AsyncHelpers.WhenAll(new[] { empty, running });
            Assert.False(result.IsCompleted);

            await WaitAsync();
            isRunning = false;

            await result;
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task WhenAll_ShouldCancelAsynchronously()
        {
            using var cts = new CancellationTokenSource();

            var empty = new ValueTask();
            var canceled = new ValueTask(Task.Delay(Timeout.Infinite, cts.Token));

            var result = AsyncHelpers.WhenAll(new[] { empty, canceled });
            Assert.False(result.IsCompleted);

            cts.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await result);
            Assert.True(result.IsCanceled);
        }

        [Fact]
        public async Task WhenAll_ShouldThrowAsyncException()
        {
            var isRunning = true;

            var empty = new ValueTask();
            var running = new ValueTask(Task.Run(async () =>
            {
                while (isRunning)
                    await WaitAsync();

                throw new TestException();
            }));

            var result = AsyncHelpers.WhenAll(new[] { empty, running });
            Assert.False(result.IsCompleted);

            await WaitAsync();
            isRunning = false;

            await Assert.ThrowsAsync<TestException>(async () => await result);
            Assert.True(result.IsFaulted);
        }

        [Fact]
        public async Task WhenAll_ShouldThrowMultipleAsyncExceptions()
        {
            var isRunning = true;

            var empty = new ValueTask();
            var running1 = new ValueTask(Task.Run(async () =>
            {
                while (isRunning)
                    await WaitAsync();

                throw new TestException("1");
            }));

            var running2 = new ValueTask(Task.Run(async () =>
            {
                while (isRunning)
                    await WaitAsync();

                throw new TestException("2");
            }));

            var result = AsyncHelpers.WhenAll(new[] { empty, running1, running2 });
            Assert.False(running1.IsCompleted);
            Assert.False(running2.IsCompleted);
            Assert.False(result.IsCompleted);

            await WaitAsync();
            isRunning = false;

            var exception = await Assert.ThrowsAsync<TestException>(async () => await result);
            Assert.True(running1.IsFaulted);
            Assert.True(running2.IsFaulted);
            Assert.True(result.IsFaulted);
            Assert.Equal("1", exception.Message);
            Assert.Equal(2, result.AsTask().Exception?.Flatten().InnerExceptions.Count);
        }

        [Fact]
        public async Task WhenAll_ShouldCompleteSynchronously()
        {
            var empty = new ValueTask();

            var result = AsyncHelpers.WhenAll(new[] { empty });
            await result;

            Assert.True(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task WhenAll_ShouldCancelSynchronously()
        {
            static async ValueTask GetCanceledValueTask() => throw new OperationCanceledException();

            var empty = new ValueTask();
            var canceled = GetCanceledValueTask();

            var result = AsyncHelpers.WhenAll(new[] { empty, canceled });
            Assert.True(result.IsCompleted);
            Assert.True(result.IsCanceled);

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await result);
        }

        [Fact]
        public async Task WhenAll_ShouldThrowSyncException()
        {
            var empty = new ValueTask();
            var fromException = new ValueTask(Task.FromException(new TestException()));

            var result = AsyncHelpers.WhenAll(new[] { empty, fromException });
            Assert.True(result.IsCompleted);
            Assert.True(result.IsFaulted);

            await Assert.ThrowsAsync<TestException>(async () => await result);
        }

        [Fact]
        public async Task WhenAll_ShouldThrowMultipleSyncExceptions()
        {
            var empty = new ValueTask();
            var fromException1 = new ValueTask(Task.FromException(new TestException("1")));
            var fromException2 = new ValueTask(Task.FromException(new TestException("2")));

            var result = AsyncHelpers.WhenAll(new[] { empty, fromException1, fromException2 });
            Assert.True(result.IsCompleted);
            Assert.True(result.IsFaulted);

            var exception = await Assert.ThrowsAsync<TestException>(async () => await result);
            Assert.Equal("1", exception.Message);
            Assert.Equal(2, result.AsTask().Exception?.Flatten().InnerExceptions.Count);
        }

        private static Task WaitAsync() => Task.Delay(100);
    }
}
