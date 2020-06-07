using System;
using TypingRealm.Domain.Common;

namespace TypingRealm.Domain.Movement
{
    public sealed class Distance : Primitive<int>
    {
        public Distance(int value) : base(value)
        {
            if (value < 0)
                throw new ArgumentException("Distance cannot have negative value.", nameof(value));
        }

        public bool IsZero => Value == 0;

        public static Distance Zero => new Distance(0);

        public static implicit operator int(Distance distance)
            => distance.Value;

        public static Distance operator +(Distance left, Distance right)
            => new Distance(left.Value + right.Value);

        public static Distance operator -(Distance left, Distance right)
            => new Distance(left.Value - right.Value);
    }
}
