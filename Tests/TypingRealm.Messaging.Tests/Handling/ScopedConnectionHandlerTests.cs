using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TypingRealm.Messaging.Handling;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Handling;

public class ScopedConnectionHandlerTests : TestsBase
{
    [Theory, AutoMoqData]
    public async Task ShouldUseServiceScopeFactoryToGetConnectionHandler(
        IConnection connection,
        IConnectionHandler handler,
        IServiceProvider serviceProvider,
        IServiceScope serviceScope,
        [Frozen] IServiceScopeFactory serviceScopeFactory,
        ScopedConnectionHandler sut)
    {
        Mock.Get(serviceScopeFactory).Setup(x => x.CreateScope())
            .Returns(serviceScope);

        Mock.Get(serviceScope).Setup(x => x.ServiceProvider)
            .Returns(serviceProvider);

        Mock.Get(serviceProvider).Setup(x => x.GetService(typeof(IConnectionHandler)))
            .Returns(handler);

        var handled = false;
        Mock.Get(handler).Setup(x => x.HandleAsync(connection, Cts.Token))
            .Returns(async () =>
            {
                while (!handled)
                {
                    await Task.Delay(10);
                }
            });

        var handling = sut.HandleAsync(connection, Cts.Token);
        await Wait();

        Mock.Get(serviceScope).Verify(x => x.Dispose(), Times.Never);

        handled = true;
        await handling;

        Mock.Get(serviceScope).Verify(x => x.Dispose());
    }
}
