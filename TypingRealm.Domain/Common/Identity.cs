using System;

namespace TypingRealm.Domain.Common
{
    /// <summary>
    /// String identity.
    /// </summary>
    public abstract class Identity : Primitive<string>
    {
        protected Identity(string value) : base(value)
        {
            if (value.Trim().Length == 0)
                throw new ArgumentException("String identity cannot be empty.");
        }
    }
}
