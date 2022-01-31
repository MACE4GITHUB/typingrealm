using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Connecting.Initializers;
using TypingRealm.Messaging.Handlers;
using TypingRealm.Messaging.Handling;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Serialization;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class TestMessageHandler : IMessageHandler<TestMessage>
{
    public ValueTask HandleAsync(ConnectedClient sender, TestMessage message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class TestResponse { }
public class TestQueryHandler : IQueryHandler<TestMessage, TestResponse>
{
    public ValueTask<TestResponse> HandleAsync(
        ConnectedClient sender,
        TestMessage queryMessage,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class TestUpdateFactory : IUpdateFactory
{
    public object GetUpdateFor(string clientId)
    {
        throw new NotImplementedException();
    }
}

public class TestConnectionInitializer : IConnectionInitializer
{
    public ValueTask<ConnectedClient> ConnectAsync(IConnection connection, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class RegistrationExtensionsTests : TestsBase
{
    private readonly IServiceProvider _provider;

    public RegistrationExtensionsTests()
    {
        _provider = new ServiceCollection()
            .AddLogging()
            .AddSerializationCore().Services
            .RegisterMessaging()
            .BuildServiceProvider();
    }

    [Theory]
    [InlineData(typeof(IConnectHook), typeof(EmptyConnectHook))]
    [InlineData(typeof(IConnectionInitializer), typeof(ConnectInitializer))]
    [InlineData(typeof(IScopedConnectionHandler), typeof(ScopedConnectionHandler))]
    [InlineData(typeof(IConnectionHandler), typeof(ConnectionHandler))]
    [InlineData(typeof(IMessageDispatcher), typeof(MessageDispatcher))]
    [InlineData(typeof(IMessageHandlerFactory), typeof(MessageHandlerFactory))]
    [InlineData(typeof(IQueryDispatcher), typeof(QueryDispatcher))]
    [InlineData(typeof(IQueryHandlerFactory), typeof(QueryHandlerFactory))]
    //[InlineData(typeof(IUpdater), typeof(AnnouncingUpdater))]
    [InlineData(typeof(IUpdater), typeof(NoUpdater))]
    [InlineData(typeof(IMessageHandler<Announce>), typeof(AnnounceHandler))]
    [InlineData(typeof(IMessageHandler<Disconnect>), typeof(DisconnectHandler))]
    public void ShouldRegisterTransientTypes(Type interfaceType, Type implementationType)
    {
        _provider.AssertRegisteredTransient(interfaceType, implementationType);
    }

    [Theory]
    [InlineData(typeof(IUpdateDetector), typeof(UpdateDetector))]
    [InlineData(typeof(IConnectedClientStore), typeof(ConnectedClientStore))]
    [InlineData(typeof(QueryHandlerMethodCache), typeof(QueryHandlerMethodCache))]
    public void ShouldRegisterSingletonTypes(Type interfaceType, Type implementationType)
    {
        _provider.AssertRegisteredSingleton(interfaceType, implementationType);
    }

    [Fact]
    public void ShouldRegisterHandler()
    {
        var provider = new ServiceCollection()
            .RegisterHandler<TestMessage, TestMessageHandler>()
            .BuildServiceProvider();

        provider.AssertRegisteredTransient(typeof(IMessageHandler<TestMessage>), typeof(TestMessageHandler));
    }

    [Fact]
    public void ShouldRegisterQueryHandler()
    {
        var provider = new ServiceCollection()
            .RegisterQueryHandler<TestMessage, TestQueryHandler, TestResponse>()
            .BuildServiceProvider();

        provider.AssertRegisteredTransient(typeof(IQueryHandler<TestMessage, TestResponse>), typeof(TestQueryHandler));
    }

    [Fact]
    public void UseUpdateFactory_ShouldRegisterUpdaterAndCustomUpdateFactoryTransient()
    {
        var provider = new ServiceCollection()
            .UseUpdateFactory<TestUpdateFactory>()
            .BuildServiceProvider();

        provider.AssertRegisteredTransient<IUpdater, Updater>();
        provider.AssertRegisteredTransient<IUpdateFactory, TestUpdateFactory>();
    }

    [Fact]
    public void UseConnectionInitializer_ShouldRegisterCustomConnectionInitializerTransient()
    {
        var provider = new ServiceCollection()
            .UseConnectionInitializer<TestConnectionInitializer>()
            .BuildServiceProvider();

        provider.AssertRegisteredTransient<IConnectionInitializer, TestConnectionInitializer>();
    }
}
