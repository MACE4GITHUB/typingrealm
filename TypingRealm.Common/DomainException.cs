using System;

namespace TypingRealm;

/// <summary>
/// Business exception thrown when domain invariant has been violated.
/// This converts to 409 status code in API terms.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
