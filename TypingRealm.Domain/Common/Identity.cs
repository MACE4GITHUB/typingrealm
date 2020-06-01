using System;
using System.Collections.Generic;

namespace TypingRealm.Domain.Common
{
    /// <summary>
    /// String identity.
    /// </summary>
    public abstract class Identity : Identity<string>
    {
        protected Identity(string value) : base(value)
        {
            if (value.Trim().Length == 0)
                throw new ArgumentException("String identity cannot be empty.");
        }
    }

    public abstract class Identity<TValue> : Primitive<TValue>
    {
        protected Identity(TValue value) : base(value)
        {
            if (EqualityComparer<TValue>.Default.Equals(value, default))
                throw new ArgumentException("Identity cannot have default value.", nameof(value));
        }
    }
}
