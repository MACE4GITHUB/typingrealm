using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace TypingRealm.Messaging.Handling;

// Register as singleton.
// TODO: Unit test (and make an interface for injection).
public sealed class QueryHandlerMethodCache
{
    private readonly ConcurrentDictionary<Tuple<Type, Type>, MethodInfo> _methods
        = new ConcurrentDictionary<Tuple<Type, Type>, MethodInfo>();

    public MethodInfo GetHandleMethod(Type queryMessageType, Type responseType)
    {
        var key = new Tuple<Type, Type>(queryMessageType, responseType);

        if (_methods.TryGetValue(key, out var methodInfo))
        {
            return methodInfo;
        }

        var method = typeof(QueryHandlerFactory).GetMethod($"HandleAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null)
            throw new InvalidOperationException("Unable to create method.");

        var newMethodInfo = method.MakeGenericMethod(queryMessageType, responseType);
        if (newMethodInfo == null)
            throw new InvalidOperationException("Unable to create MethodInfo.");

        _methods.TryAdd(key, newMethodInfo);
        return newMethodInfo;
    }
}
