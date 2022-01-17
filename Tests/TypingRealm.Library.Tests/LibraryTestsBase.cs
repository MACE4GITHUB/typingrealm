using TypingRealm.Testing;

namespace TypingRealm.Library.Tests
{
    public abstract class LibraryTestsBase : TestsBase
    {
        protected LibraryTestsBase() : base(AutoDomainDataAttribute.CreateFixture())
        {
        }
    }
}
