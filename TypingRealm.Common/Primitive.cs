using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TypingRealm;

/// <summary>
/// Primitive base class that can be used instead of to avoid primitive obsession,
/// like BookDescription, ProfileName etc. It's also a basis for the
/// <see cref="Identity"/> class.
/// When value is struct - null values are allowed. For classes null values
/// would lead to <see cref="ArgumentNullException"/>.
/// </summary>
#pragma warning disable S4035 // Classes implementing "IEquatable<T>" should be sealed
public abstract class Primitive<TValue>
#pragma warning restore S4035
    : IEquatable<Primitive<TValue>>,
    IEqualityComparer<Primitive<TValue>>
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
        if (GetType() != obj?.GetType())
            return false;

        return Equals(this, obj as Primitive<TValue>);
    }

    public bool Equals(Primitive<TValue>? other)
    {
        if (GetType() != other?.GetType())
            return false;

        return Equals(this, other);
    }

    public bool Equals(Primitive<TValue>? x, Primitive<TValue>? y)
    {
        if (x == null && y == null)
            return true;

        if (x == null || y == null)
            return false;

        return EqualityComparer<TValue>.Default.Equals(x.Value, y.Value);
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

    public int GetHashCode([DisallowNull] Primitive<TValue> obj)
    {
        return obj.GetHashCode();
    }

    public sealed override string? ToString()
    {
        return Value!.ToString();
    }

    public static bool operator ==(Primitive<TValue>? left, Primitive<TValue>? right)
    {
        return left is null
            ? right is null
            : left.Equals(right);
    }

    public static bool operator !=(Primitive<TValue>? left, Primitive<TValue>? right)
        => !(left == right);

#pragma warning disable CA2225 // It's a base class, we cannot introduce this method.
    public static implicit operator TValue(Primitive<TValue> identity)
#pragma warning restore CA2225
        => identity.Value;
}
