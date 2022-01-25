using TypingRealm.Messaging.Serialization.Json;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests.Json;

public class JsonSerializedMessageTests : TestsBase
{
    [Fact]
    public void JsonSerializedMessage()
    {
        AssertSerializable<JsonSerializedMessage>();

        var sut = new JsonSerializedMessage("typeId", "json");
        Assert.Equal("typeId", sut.TypeId);
        Assert.Equal("json", sut.Json);

        sut = new JsonSerializedMessage
        {
            TypeId = "typeId",
            Json = "json"
        };
        Assert.Equal("typeId", sut.TypeId);
        Assert.Equal("json", sut.Json);
    }
}
