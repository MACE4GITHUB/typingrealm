using System;

namespace TypingRealm.Testing
{
    public class TestException : Exception
    {
        public TestException() { }
        public TestException(string message) : base(message) { }
    }
}
