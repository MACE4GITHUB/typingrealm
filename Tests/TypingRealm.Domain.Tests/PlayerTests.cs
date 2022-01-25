using System;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Domain.Movement;
using TypingRealm.Domain.Tests.Customizations;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Domain.Tests;

public class PlayerTests : TestsBase
{
    public PlayerTests()
    {
        Fixture.Customize(new DomainCustomization());
    }

    // TODO: Test constructor and setting all fields properly.

    [Theory, AutoDomainData]
    public void MoveToLocation_ShouldChangeLocation(
        [Frozen] Mock<ILocationStore> store,
        LocationId locationId,
        Player player)
    {
        var location = Create<Location>(
            new LocationWithAnotherLocation(locationId));

        var state = player.GetState();

        store.Setup(x => x.Find(state.LocationId))
            .Returns(location);

        player.MoveToLocation(locationId);
        Assert.Equal(locationId, player.GetState().LocationId);
    }

    [Theory, AutoDomainData]
    public void MoveToLocation_ShouldThrow_WhenAlreadyAtLocation(
        [Frozen] Mock<ILocationStore> store,
        Player player)
    {
        var state = player.GetState();

        var location = Create<Location>(
            new LocationWithAnotherLocation(state.LocationId));

        store.Setup(x => x.Find(state.LocationId))
            .Returns(location);

        Assert.Throws<InvalidOperationException>(
            () => player.MoveToLocation(state.LocationId));
    }

    [Theory, AutoDomainData]
    public void MoveToLocation_ShouldThrow_WhenCurrentLocationDoesNotExist(
        [Frozen] Mock<ILocationStore> store,
        Player player)
    {
        var state = player.GetState();

        store.Setup(x => x.Find(state.LocationId))
            .Returns<Location>(null);

        Assert.Throws<InvalidOperationException>(
            () => player.MoveToLocation(Create<LocationId>()));
    }

    [Theory, AutoDomainData]
    public void MoveToLocation_ShouldThrow_WhenLocationDoesNotExist(
        [Frozen] Mock<ILocationStore> store,
        Player player,
        LocationId locationId)
    {
        var state = player.GetState();

        var location = Create<Location>(
            new LocationWithAnotherLocation(locationId));

        store.Setup(x => x.Find(state.LocationId))
            .Returns(location);

        store.Setup(x => x.Find(locationId))
            .Returns<Location>(null);

        Assert.Throws<InvalidOperationException>(
            () => player.MoveToLocation(locationId));
    }

    [Theory, AutoDomainData]
    public void MoveToLocation_ShouldThrow_WhenCannotMoveToThisLocationFromCurrentLocation(
        Player player,
        LocationId locationId)
    {
        Assert.Throws<InvalidOperationException>(
            () => player.MoveToLocation(locationId));
    }
}
