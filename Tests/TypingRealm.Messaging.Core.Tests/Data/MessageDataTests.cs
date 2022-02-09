using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class MessageDataTests : TestsBase
{
    [Fact]
    public void ShouldBeSerializable()
    {
        AssertSerializable<MessageData>();
    }
}
