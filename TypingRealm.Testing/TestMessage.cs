using TypingRealm.Messaging;

namespace TypingRealm.Testing;

[Message]
public class TestMessage
{
#pragma warning disable CS8618
    public TestMessage() { }
#pragma warning restore CS8618
    public TestMessage(string value) => Value = value;

    public string Value { get; set; }
}
