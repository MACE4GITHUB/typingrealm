using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TypingRealm
{
    public static class AsyncHelpers
    {
        /// <summary>
        /// Waits when all value tasks will complete.
        /// </summary>
        /// <param name="valueTasks"></param>
        /// <returns></returns>
        public static ValueTask WhenAll(IEnumerable<ValueTask> valueTasks)
        {
            // We cannot await Task to create ValueTask, because then we lose
            // AggregateException (InnerExceptions).
            if (valueTasks.All(vt => vt.IsCompletedSuccessfully))
                return default;

            return new ValueTask(Task.WhenAll(valueTasks
                .Where(vt => !vt.IsCompletedSuccessfully)
                .Select(vt => vt.AsTask())));
        }
    }
}
