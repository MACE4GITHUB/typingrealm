using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Tests.SpecimenBuilders;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connecting;

public class ConnectedClientStoreTests : MessagingTestsBase
{
    [Theory, AutoMoqData]
    public void Add_ShouldThrow_WhenAlreadyAdded(ConnectedClientStore sut)
    {
        Fixture.Customizations.Add(
            new ClientWithIdBuilder(Create<string>()));

        sut.Add(Create<ConnectedClient>());

        Assert.Throws<ClientAlreadyConnectedException>(
            () => sut.Add(Create<ConnectedClient>()));
    }

    [Theory, AutoMoqData]
    public void Add_ShouldAddClient_WhenNotAdded(
        ConnectedClient client,
        ConnectedClientStore sut)
    {
        sut.Add(client);

        var result = sut.Find(client.ClientId);
        Assert.NotNull(result);
        Assert.Equal(client, result);
    }

    [Theory, SingleGroupData]
    public void Add_ShouldMarkForUpdate_WhenAdded(
        [Frozen] Mock<IUpdateDetector> updateDetector,
        ConnectedClient client,
        ConnectedClientStore sut)
    {
        sut.Add(client);
        updateDetector.Verify(x => x.MarkForUpdate(It.Is<IEnumerable<string>>(
            x => x.Count() == 1
            && x.First() == client.Group)));
    }

    [Theory, MultiGroupData]
    public void Add_ShouldMarkForUpdateMultipleGroups_WhenAdded(
        [Frozen] Mock<IUpdateDetector> updateDetector,
        ConnectedClient client,
        ConnectedClientStore sut)
    {
        sut.Add(client);
        updateDetector.Verify(x => x.MarkForUpdate(It.Is<IEnumerable<string>>(
            x => x.Count() == 3
            && x.SequenceEqual(client.Groups))));
    }

    [Theory, MultiGroupData]
    public void Add_ShouldMarkForUpdateMultipleGroupsAndSingleGroup_WhenAdded(
        [Frozen] Mock<IUpdateDetector> updateDetector,
        ConnectedClient client,
        string group,
        ConnectedClientStore sut)
    {
        // Possibly improve this unit test.
        client.Group = group;

        sut.Add(client);
        updateDetector.Verify(x => x.MarkForUpdate(It.Is<IEnumerable<string>>(
            x => x.Count() == 4
            && x.SequenceEqual(client.Groups)
            && x.Contains(group))));
    }

    [Theory, AutoMoqData]
    public void Find_ShouldReturnNull_WhenNotAdded(ConnectedClientStore sut)
    {
        Assert.Null(sut.Find(Create<string>()));
    }

    [Theory, AutoMoqData]
    public void Find_ShouldFind_WhenAdded(
        ConnectedClient client,
        ConnectedClientStore sut)
    {
        sut.Add(client);
        var found = sut.Find(client.ClientId);

        Assert.Equal(client, found);
    }

    public enum FindType
    {
        RegularMethod,
        ExtensionParamsMethod
    }
    [Theory]
    [InlineAutoMoqData(FindType.RegularMethod)]
    [InlineAutoMoqData(FindType.ExtensionParamsMethod)]
    public void ShouldFindAllClientsInGroups(FindType findType, ConnectedClientStore sut)
    {
        var group1 = Fixture.Create<string>();
        var group2 = Fixture.Create<string>();
        var group3 = Fixture.Create<string>();

        var clientsInGroup1 = Fixture.Build<ConnectedClient>()
            .With(x => x.Group, group1)
            .CreateMany(2);

        var clientsInGroup2 = Fixture.Build<ConnectedClient>()
            .With(x => x.Group, group2)
            .CreateMany(2);

        var clientsInGroup3 = Fixture.Build<ConnectedClient>()
            .With(x => x.Group, group3)
            .CreateMany(2);

        var clientInGroups1And2 = CreateMultiGroupClient(new[] { group1, group2 });
        var clientInGroups123 = CreateMultiGroupClient(new[] { group1, group2 });
        clientInGroups123.Group = group3;

        foreach (var client in clientsInGroup1
            .Concat(clientsInGroup2)
            .Concat(clientsInGroup3)
            .Append(clientInGroups1And2)
            .Append(clientInGroups123))
        {
            sut.Add(client);
        }

        var result = findType switch
        {
            FindType.RegularMethod => sut.FindInGroups(new[] { group1, group3 }),
            FindType.ExtensionParamsMethod => sut.FindInGroups(group1, group3),
            _ => throw new InvalidOperationException()
        };
        Assert.Equal(6, result.Count());
        Assert.True(clientsInGroup1.All(x => result.Contains(x)));
        Assert.True(clientsInGroup2.All(x => !result.Contains(x)));
        Assert.True(clientsInGroup3.All(x => result.Contains(x)));
        Assert.Contains(clientInGroups1And2, result);
        Assert.Contains(clientInGroups123, result);
    }

    [Theory, AutoMoqData]
    public void Remove_ShouldRemoveClient(
        ConnectedClient client,
        ConnectedClientStore sut)
    {
        sut.Add(client);
        Assert.Equal(client, sut.Find(client.ClientId));

        sut.Remove(client.ClientId);
        Assert.Null(sut.Find(client.ClientId));
    }

    [Theory, SingleGroupData]
    public void Remove_ShouldMarkForUpdate_WhenRemoved(
        [Frozen] Mock<IUpdateDetector> updateDetector,
        ConnectedClient client,
        ConnectedClientStore sut)
    {
        sut.Add(client);
        updateDetector.Verify(x => x.MarkForUpdate(It.Is<IEnumerable<string>>(
            x => x.Count() == 1 && x.First() == client.Group)), Times.Once);

        sut.Remove(client.ClientId);
        updateDetector.Verify(x => x.MarkForUpdate(It.Is<IEnumerable<string>>(
            x => x.Count() == 1 && x.First() == client.Group)), Times.Exactly(2));
    }

    [Theory, AutoMoqData]
    public void Remove_ShouldMarkForUpdateMultiGroupClient_WhenRemoved(
        [Frozen] Mock<IUpdateDetector> updateDetector,
        string[] groups,
        ConnectedClientStore sut)
    {
        var client = CreateMultiGroupClient(groups);

        sut.Add(client);
        updateDetector.Verify(x => x.MarkForUpdate(It.Is<IEnumerable<string>>(
            x => x.SequenceEqual(groups))), Times.Once);

        sut.Remove(client.ClientId);
        updateDetector.Verify(x => x.MarkForUpdate(It.Is<IEnumerable<string>>(
            x => x.SequenceEqual(groups))), Times.Exactly(2));
    }

    [Theory, AutoMoqData]
    public void ShouldNotThrow_WhenRemovingNonExistingClient(ConnectedClientStore sut)
    {
        sut.Remove(Create<string>());
    }

    [Theory, AutoMoqData]
    public async Task ConcurrencyTest(
        string clientId,
        ConnectedClientStore sut)
    {
        Fixture.Customizations.Add(new ClientWithIdBuilder(clientId));

        var tasks = new List<Task>();
        for (var i = 0; i < 100; i++)
        {
            tasks.Add(new Task(() => sut.Add(Create<ConnectedClient>())));
            tasks.Add(new Task(() => sut.Remove(clientId)));
            tasks.Add(new Task(() => sut.Find(clientId)));
        }

        Parallel.ForEach(tasks, task => task.Start());
        try { await Task.WhenAll(tasks); } catch (ClientAlreadyConnectedException) { }
        try { sut.Add(Create<ConnectedClient>()); } catch (ClientAlreadyConnectedException) { }
        Assert.NotNull(sut.Find(clientId));
    }
}
