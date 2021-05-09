using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Adds CancellationToken to the task. If CancellationToken is canceled
        /// before the task completes - OperationCanceledException is thrown.
        /// </summary>
        /// <param name="task">Task to which cancellation will be attached.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="OperationCanceledException">Thrown when supplied
        /// cancellation token is canceled before the task completes.</exception>
        public static async Task WithCancellationAsync(
            this Task task, CancellationToken cancellationToken)
        {
            using var localCts = new CancellationTokenSource();
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, localCts.Token);

            var delayTask = Task.Delay(Timeout.Infinite, combinedCts.Token);
            var completedTask = await Task.WhenAny(task, delayTask).ConfigureAwait(false);
            localCts.Cancel();

            if (completedTask == delayTask)
                throw new OperationCanceledException(cancellationToken);

            await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Adds CancellationToken to the task. If CancellationToken is canceled
        /// before the task completes - OperationCanceledException is thrown.
        /// </summary>
        /// <typeparam name="TResult">Result of the task.</typeparam>
        /// <param name="task">Task to which cancellation will be attached.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result of the task if the task completes before cancellation.</returns>
        /// <exception cref="OperationCanceledException">Thrown when supplied
        /// cancellation token is canceled before the task completes.</exception>
        public static async Task<TResult> WithCancellationAsync<TResult>(
            this Task<TResult> task, CancellationToken cancellationToken)
        {
            using var localCts = new CancellationTokenSource();
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, localCts.Token);

            var delayTask = Task.Delay(Timeout.Infinite, combinedCts.Token);
            var completedTask = await Task.WhenAny(task, delayTask).ConfigureAwait(false);
            localCts.Cancel();

            if (completedTask == delayTask)
                throw new OperationCanceledException(cancellationToken);

            return await task.ConfigureAwait(false);
        }

        // TODO: Unit test this.
        public static async Task WithTimeoutAsync(
            this Task task, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);

            await task.WithCancellationAsync(cts.Token)
                .ConfigureAwait(false);
        }

        // TODO: Unit test this.
        public static async Task<TResult> WithTimeoutAsync<TResult>(
            this Task<TResult> task, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);

            return await task.WithCancellationAsync(cts.Token)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Handles exception thrown during task execution.
        /// </summary>
        /// <typeparam name="TException">Handled exception type.</typeparam>
        /// <param name="task">Task that might throw given exception.</param>
        /// <param name="onException">Action that will execute when exception is thrown.</param>
        /// <param name="handleOnCapturedContext">Handles exception on captured
        /// context if true, uses ConfigureAwait(false) otherwise. False by default.</param>
        public static async Task HandleExceptionAsync<TException>(
            this Task task, Action<TException> onException, bool handleOnCapturedContext = false)
            where TException : Exception
        {
            try
            {
                await task.ConfigureAwait(handleOnCapturedContext);
            }
            catch (TException exception)
            {
                onException(exception);
            }
        }

        /// <summary>
        /// Handles <see cref="OperationCanceledException"/> exception thrown
        /// during task execution.
        /// </summary>
        /// <param name="task">Task that might throw <see cref="OperationCanceledException"/> exception.</param>
        /// <param name="onCancellationException">Action that will execute when
        /// <see cref="OperationCanceledException"/> exception is thrown.</param>
        /// <param name="handleOnCapturedContext">Handles exception on captured
        /// context if true, uses ConfigureAwait(false) otherwise. False by default.</param>
        public static Task HandleCancellationAsync(
            this Task task,
            Action<OperationCanceledException> onCancellationException,
            bool handleOnCapturedContext = false)
            => HandleExceptionAsync(task, onCancellationException, handleOnCapturedContext);

        /// <summary>
        /// Swallows <see cref="OperationCanceledException"/> on task.
        /// </summary>
        /// <param name="task">Task that can throw <see cref="OperationCanceledException"/>.</param>
        public static Task SwallowCancellationAsync(this Task task)
            => HandleCancellationAsync(task, _ => { });
    }
}
