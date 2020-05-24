using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Handling;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Handling
{
    public class MessageHandlerFactoryTests : TestsBase
    {
        [Theory, AutoMoqData]
        public void ShouldGetAllHandlers(
            [Frozen]Mock<IServiceProvider> services,
            MessageHandlerFactory sut)
        {
            var handlers = Fixture.CreateMany<IMessageHandler<TestMessage>>();
            services.Setup(x => x.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
                .Returns(handlers);

            var result = sut.GetHandlersFor<TestMessage>();

            Assert.Equal(handlers, result);
        }
    }
}
