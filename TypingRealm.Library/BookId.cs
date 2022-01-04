using System;
using TypingRealm.Common;

namespace TypingRealm.Library
{
    public sealed class BookId : Identity
    {
        public BookId(string value) : base(value)
        {
        }

        public static BookId New() => new(Guid.NewGuid().ToString());
    }
}
