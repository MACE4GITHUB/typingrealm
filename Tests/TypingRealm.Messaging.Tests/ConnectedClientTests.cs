using System.Collections.Generic;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Tests.SpecimenBuilders;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class ConnectedClientTests : MessagingTestsBase
{
    [Theory, AutoMoqData]
    public void ShouldBeCreatedUsingConstructor(
        string clientId,
        IConnection connection,
        string group,
        IUpdateDetector updateDetector)
    {
        var client = new ConnectedClient(clientId, connection, updateDetector, group);

        Assert.Equal(clientId, client.ClientId);
        Assert.Equal(connection, client.Connection);
        Assert.Equal(group, client.Group);
        Assert.Equal(updateDetector, GetPrivateField(client, "_updateDetector"));
    }

    [Theory, AutoMoqData]
    public void ShouldNotMarkForUpdateOnCreation(IUpdateDetector updateDetector)
    {
        _ = new ConnectedClient(
            Create<string>(),
            Create<IConnection>(),
            updateDetector,
            Create<string>());

        Mock.Get(updateDetector).Verify(x => x.MarkForUpdate(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Theory, SingleGroupData]
    public void ShouldMarkPreviousAndNewGroupsForUpdate_WhenGroupIsChanged(
        [Frozen] Mock<IUpdateDetector> updateDetector,
        string newGroup,
        ConnectedClient sut)
    {
        var oldGroup = sut.Group;
        sut.Group = newGroup;

        VerifyMarkedForUpdate(updateDetector, oldGroup);
        VerifyMarkedForUpdate(updateDetector, newGroup);
    }

    [Theory, AutoMoqData]
    public void ShouldNotMarkNewGroupForUpdate_WhenSetToTheSameGroup(
        [Frozen] Mock<IUpdateDetector> updateDetector,
        string newGroup,
        ConnectedClient sut)
    {
        VerifyMarkedForUpdate(updateDetector, newGroup, Times.Never());

        sut.Group = newGroup;
        VerifyMarkedForUpdate(updateDetector, newGroup, Times.Once());

        sut.Group = newGroup;
        VerifyMarkedForUpdate(updateDetector, newGroup, Times.Once()); // Still once.
    }
}
