using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Domain.Messages;
using TypingRealm.Messaging;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Domain.Tests
{
    public class JoinHandlerTests : TestsBase
    {
        public JoinHandlerTests()
        {
            Fixture.Customize(new DomainCustomization());
        }

        [Theory, AutoDomainData]
        public async Task ShouldThrow_WhenAlreadyJoined(
            ConnectedClient sender,
            [Frozen]Mock<IPlayerRepository> playerRepository,
            JoinHandler sut)
        {
            playerRepository.Setup(x => x.FindByClientId(sender.ClientId))
                .Returns(Create<Player>());

            await AssertThrowsAsync<InvalidOperationException>(
                () => sut.HandleAsync(sender, Create<Join>(), Cts.Token));
        }
    }
}
