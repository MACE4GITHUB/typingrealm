using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Tests
{
    public class AsyncHelpersTests : TestsBase
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task WhenAll_ShouldComplete(bool isAsynchronous)
        {
            var isRunning = true;

            var completed1 = new ValueTask();
            var completed2 = new ValueTask();
            var running1 = new ValueTask(Task.Run(async () =>
            {
                while (isRunning)
                    await Wait();
            }));
            var running2 = new ValueTask(Task.Run(async () =>
            {
                while (isRunning)
                    await Wait();
            }));

            var result = isAsynchronous
                ? AsyncHelpers.WhenAll(new[] { completed1, running1, running2 })
                : AsyncHelpers.WhenAll(new[] { completed1, completed2 });

            if (isAsynchronous)
            {
                await Wait();
                Assert.False(result.IsCompleted);
                isRunning = false;
            }

            await result;
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task WhenAll_ShouldCancel(bool isAsynchronous)
        {
            var canceled1 = new ValueTask(Task.FromCanceled(new CancellationToken(true)));
            var canceled2 = new ValueTask(Task.FromCanceled(new CancellationToken(true)));
            var running1 = new ValueTask(Task.Delay(Timeout.Infinite, Cts.Token));
            var running2 = new ValueTask(Task.Delay(Timeout.Infinite, Cts.Token));

            var result = isAsynchronous
                ? AsyncHelpers.WhenAll(new[] { canceled1, running1, running2 })
                : AsyncHelpers.WhenAll(new[] { canceled1, canceled2 });

            if (isAsynchronous)
            {
                await Wait();
                Assert.False(result.IsCompleted);
                Cts.Cancel();
            }

            await AssertThrowsAsync<TaskCanceledException>(() => result);
            Assert.True(result.IsCanceled);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task WhenAll_ShouldThrow(bool isAsynchronous)
        {
            var isRunning = true;

            var thrown1 = new ValueTask(Task.FromException(Create<TestException>()));
            var thrown2 = new ValueTask(Task.FromException(Create<TestException>()));
            var running1 = new ValueTask(Task.Run(async () =>
            {
                while (isRunning)
                    await Wait();

                throw new TestException();
            }));
            var running2 = new ValueTask(Task.Run(async () =>
            {
                while (isRunning)
                    await Wait();

                throw new TestException();
            }));

            var result = isAsynchronous
                ? AsyncHelpers.WhenAll(new[] { thrown1, running1, running2 })
                : AsyncHelpers.WhenAll(new[] { thrown1, thrown2 });

            if (isAsynchronous)
            {
                await Wait();
                Assert.False(result.IsCompleted);
                isRunning = false;
            }

            await AssertThrowsAsync<TestException>(() => result);
            Assert.True(result.IsFaulted);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task WhenAll_ShouldThrowMultipleExceptions(bool isAsynchronous)
        {
            var isRunning = true;

            var thrown1 = new ValueTask(Task.FromException(Create<TestException>()));
            var thrown2 = new ValueTask(Task.FromException(Create<TestException>()));
            var running1 = new ValueTask(Task.Run(async () =>
            {
                while (isRunning)
                    await Wait();

                throw new TestException("1");
            }));
            var running2 = new ValueTask(Task.Run(async () =>
            {
                while (isRunning)
                    await Wait();

                throw new TestException("2");
            }));

            var result = isAsynchronous
                ? AsyncHelpers.WhenAll(new[] { running1, running2, thrown1 })
                : AsyncHelpers.WhenAll(new[] { thrown1, thrown2 });

            if (isAsynchronous)
            {
                await Wait();
                Assert.False(running1.IsCompleted);
                Assert.False(running2.IsCompleted);
                Assert.False(result.IsCompleted);

                isRunning = false;
            }

            var exception = await AssertThrowsAsync<TestException>(() => result);

            if (isAsynchronous)
            {
                Assert.True(running1.IsFaulted);
                Assert.True(running2.IsFaulted);
                Assert.Equal("1", exception.Message); // First exception.
            }

            Assert.True(result.IsFaulted);
            var allThrown = result.AsTask().Exception!.Flatten().InnerExceptions.ToList();

            if (isAsynchronous)
            {
                Assert.Equal(3, allThrown.Count);
                Assert.Contains(allThrown, x => x.Message == "2");
            }
            else
            {
                Assert.Equal(2, allThrown.Count);
            }
        }
    }
}
