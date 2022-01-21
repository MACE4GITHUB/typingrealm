using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.TextProcessing.Tests;

public class TextProcessorTests
{
    [Fact]
    public void AddTextProcessing_ShouldRegisterAsSingleton()
    {
        var serviceProvider = new ServiceCollection()
            .AddTextProcessing()
            .BuildServiceProvider();

        serviceProvider.AssertRegisteredSingleton<ITextProcessor, TextProcessor>();
    }
}
