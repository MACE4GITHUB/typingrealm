using System;
using System.Collections.Generic;

namespace TypingRealm.Domain.Common
{
    public abstract class Identity<TValue> : Primitive<TValue>
    {
        protected Identity(TValue value) : base(value)
        {
            if (EqualityComparer<TValue>.Default.Equals(value, default))
                throw new ArgumentException("Identity cannot have default value.", nameof(value));
        }
    }
}
