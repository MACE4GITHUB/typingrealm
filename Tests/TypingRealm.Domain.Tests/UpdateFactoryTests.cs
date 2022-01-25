using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Domain.Messages;
using TypingRealm.Domain.Tests.Customizations;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using Xunit;

namespace TypingRealm.Domain.Tests;

public class UpdateFactoryTests
{
    [Theory]
    [AutoDomainData]
    public void ShouldThrow_WhenClientIdIsInvalid(UpdateFactory sut)
    {
        Assert.Throws<ArgumentNullException>(
            () => sut.GetUpdateFor(null!));

        Assert.Throws<ArgumentException>(
            () => sut.GetUpdateFor(string.Empty));
    }

    [Theory, AutoDomainData]
    public void ShouldThrow_WhenPlayerDoesNotExist(
        PlayerId playerId,
        [Frozen] Mock<IPlayerRepository> playerRepository,
        UpdateFactory sut)
    {
        playerRepository.Setup(x => x.Find(playerId))
            .Returns<Player>(null);

        Assert.Throws<InvalidOperationException>(
            () => sut.GetUpdateFor(playerId));
    }

    [Theory, AutoDomainData]
    public void ShouldCreateUpdate_WithCurrentLocation(
        [Frozen] Mock<IPlayerRepository> playerRepository,
        Player player,
        UpdateFactory sut)
    {
        var state = player.GetState();

        playerRepository.Setup(x => x.Find(state.PlayerId))
            .Returns(player);

        var update = (Update)sut.GetUpdateFor(state.PlayerId);

        Assert.Equal(state.LocationId, update.LocationId);
    }

    [Theory, AutoDomainData]
    public void ShouldCreateUpdate_WithVisiblePlayers_FromConnection(
        [Frozen] Mock<IPlayerRepository> playerRepository,
        [Frozen] Mock<IConnectedClientStore> store,
        List<ConnectedClient> connectedClients,
        Player player,
        UpdateFactory sut)
    {
        var state = player.GetState();

        playerRepository.Setup(x => x.Find(state.PlayerId))
            .Returns(player);

        store.Setup(x => x.FindInGroups(It.Is<IEnumerable<string>>(
            en => en.Single() == $"location_{state.LocationId}")))
            .Returns(connectedClients);

        var update = (Update)sut.GetUpdateFor(state.PlayerId);

        Assert.NotEmpty(connectedClients);
        Assert.Equal(connectedClients.Count, update.VisiblePlayers.Count());
        foreach (var client in connectedClients)
        {
            Assert.Contains(client.ClientId, update.VisiblePlayers);
        }
    }
}
