using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class MessageDataTests : TestsBase
{
    [Theory, AutoMoqData]
    public void ShouldBeSerializable(
        string data, string typeId, MessageMetadata metadata)
    {
        AssertSerializable<MessageData>();

        var sut = new MessageData();
        Assert.Null(sut.Data);
        Assert.Null(sut.TypeId);
        Assert.Null(sut.Metadata);

        sut.Data = data;
        sut.TypeId = typeId;
        sut.Metadata = metadata;
        Assert.Equal(data, sut.Data);
        Assert.Equal(typeId, sut.TypeId);
        Assert.Equal(metadata, sut.Metadata);

        sut.Data = null!;
        sut.TypeId = null!;
        Assert.Null(sut.Data);
        Assert.Null(sut.TypeId);

        // Metadata can be null.
        sut.Metadata = null;
        Assert.Null(sut.Metadata);
    }
}
