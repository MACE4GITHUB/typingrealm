using TypingRealm.Testing;

namespace TypingRealm.TextProcessing.Tests
{
    public abstract class TextProcessingTestsBase : TestsBase
    {
        protected TextProcessingTestsBase() : base(AutoDomainDataAttribute.CreateFixture())
        {
        }
    }

}
