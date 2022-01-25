using System;

namespace TypingRealm.Testing;

/// <summary>
/// Throw this exception when you need to test custom exception handling.
/// </summary>
public class TestException : Exception
{
    public TestException() { }
    public TestException(string message) : base(message) { }
}
