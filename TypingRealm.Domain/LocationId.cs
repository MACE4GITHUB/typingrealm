using System;
using TypingRealm.Domain.Common;

namespace TypingRealm.Domain
{
    public sealed class LocationId : Identity<string>
    {
        public LocationId(string value) : base(value)
        {
            if (value.Trim().Length == 0)
                throw new ArgumentException("Identity cannot be empty.", nameof(value));
        }
    }
}
