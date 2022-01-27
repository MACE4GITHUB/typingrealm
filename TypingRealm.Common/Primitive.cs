using System;
using System.Collections.Generic;

namespace TypingRealm;

public abstract class Primitive<TValue>
{
    protected Primitive(TValue value)
    {
        if (!typeof(TValue).IsValueType && value is null)
            throw new ArgumentNullException(nameof(value));

        Value = value;
    }

    public TValue Value { get; }

    public sealed override bool Equals(object? obj)
    {
        return GetType() == obj?.GetType() // Test that types are exactly the same.
            && obj is Primitive<TValue> other
            && EqualityComparer<TValue>.Default.Equals(Value, other.Value);
    }

    public sealed override int GetHashCode()
    {
        var typeHashCode = GetType().GetHashCode();

        unchecked // Overflow is fine.
        {
            var hash = 17;
            hash = (hash * 23) + Value?.GetHashCode() ?? 0;
            hash = (hash * 23) + typeHashCode;
            return hash;
        }
    }

    public sealed override string? ToString()
    {
        return Value!.ToString();
    }

    public static bool operator ==(Primitive<TValue>? left, Primitive<TValue>? right)
    {
        if (left is null)
        {
            if (right is null)
                return true;

            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Primitive<TValue>? left, Primitive<TValue>? right)
        => !(left == right);

    public static implicit operator TValue(Primitive<TValue> identity)
        => identity.Value;
}
