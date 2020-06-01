using System;
using System.Collections.Generic;

namespace TypingRealm.Domain.Common
{
    public abstract class Identity<TValue>
    {
        protected Identity(TValue value)
        {
            if (EqualityComparer<TValue>.Default.Equals(value, default))
                throw new ArgumentException("Identity cannot have default value.", nameof(value));

            Value = value;
        }

        public TValue Value { get; }

        public sealed override bool Equals(object? obj)
        {
            return GetType() == obj?.GetType() // Test that types are exactly the same.
                && obj is Identity<TValue> other
                && EqualityComparer<TValue>.Default.Equals(Value, other.Value);
        }

        public sealed override int GetHashCode()
        {
            return Value!.GetHashCode();
        }

        public override string? ToString()
        {
            return Value!.ToString();
        }

        public static bool operator ==(Identity<TValue> left, Identity<TValue> right)
        {
            if (left is null)
            {
                if (right is null)
                    return true;

                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(Identity<TValue> left, Identity<TValue> right)
            => !(left == right);

        public static implicit operator TValue(Identity<TValue> identity)
            => identity.Value;
    }
}
