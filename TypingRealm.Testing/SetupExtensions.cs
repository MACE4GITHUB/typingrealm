using System;
using System.Threading.Tasks;
using Moq.Language.Flow;

namespace TypingRealm.Testing;

public static class SetupExtensions
{
    public static void ThrowsAsync<T>(this ISetup<T, ValueTask> setup, Exception exception)
        where T : class
    {
        setup.Returns(new ValueTask(Task.FromException(exception)));
    }
}
