using System;
using System.Collections.Generic;
using Moq;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Protobuf.Tests;

public class ProtobufFieldNumberCacheTests
{
    private class ATest { }
    private class BTest { }

    [Theory, AutoMoqData]
    public void ShouldAssignNumberedKeys_WithoutOrdering_StartingFromOne(
        Mock<IMessageTypeCache> cache)
    {
        var types = new Dictionary<string, Type>
            {
                { "B", typeof(BTest) },
                { "A", typeof(ATest) }
            };

        cache.Setup(x => x.GetAllTypes()).Returns(types);
        var sut = new ProtobufFieldNumberCache(cache.Object);

        Assert.Equal(1, sut.GetFieldNumber(typeof(BTest)));
        Assert.Equal(2, sut.GetFieldNumber(typeof(ATest)));
        Assert.Throws<KeyNotFoundException>(() => sut.GetFieldNumber(typeof(object)));

        Assert.Equal(typeof(BTest), sut.GetTypeByFieldNumber(1));
        Assert.Equal(typeof(ATest), sut.GetTypeByFieldNumber(2));
        Assert.Throws<KeyNotFoundException>(() => sut.GetTypeByFieldNumber(0));
    }
}
