using System.Collections.Generic;

namespace TypingRealm
{
    public static class EnumerableHelpers
    {
        public static IEnumerable<T> AsEnumerable<T>(T item)
        {
            yield return item;
        }
    }
}
