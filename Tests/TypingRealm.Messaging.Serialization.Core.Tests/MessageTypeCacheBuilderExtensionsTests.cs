using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests;

[Message]
public class AMessage { }

[Message]
public class BMessage { }

public class MessageTypeCacheBuilderExtensionsTests
{
    [Fact]
    public void ShouldAddAllMessageTypesFromAssembly()
    {
        var services = new ServiceCollection();
        var sut = new MessageTypeCacheBuilder(services);

        sut.AddMessageTypesFromAssembly(typeof(AMessage).Assembly);
        var cache = services.BuildServiceProvider()
            .GetRequiredService<IMessageTypeCache>();

        var messages = cache.GetAllTypes().Select(x => x.Value).ToList();
        Assert.True(messages.Count >= 2);
        Assert.Contains(typeof(AMessage), messages);
        Assert.Contains(typeof(BMessage), messages);

        foreach (var message in messages)
        {
            Assert.Equal(typeof(AMessage).Assembly, message.Assembly);
            Assert.NotNull(message.GetCustomAttribute<MessageAttribute>());
        }
    }
}
