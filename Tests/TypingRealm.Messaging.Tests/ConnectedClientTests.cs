using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests
{
    public class ConnectedClientTests
    {
        [Theory, AutoMoqData]
        public void ShouldBeCreatedUsingConstructor(
            string clientId,
            IConnection connection,
            string group,
            Mock<IUpdateDetector> updateDetector)
        {
            var client = new ConnectedClient(clientId, connection, group, updateDetector.Object);

            Assert.Equal(clientId, client.ClientId);
            Assert.Equal(connection, client.Connection);
            Assert.Equal(group, client.Group);

            client.Group = "another";
            updateDetector.Verify(x => x.MarkForUpdate("another"));
        }

        [Theory, AutoMoqData]
        public void ShouldNotMarkForUpdateOnCreation(
            string clientId,
            IConnection connection,
            string group,
            Mock<IUpdateDetector> updateDetector)
        {
            _ = new ConnectedClient(clientId, connection, group, updateDetector.Object);

            updateDetector.Verify(x => x.MarkForUpdate(It.IsAny<string>()), Times.Never);
        }

        [Theory, AutoMoqData]
        public void ShouldMarkPreviousAndNewGroupsForUpdate(
            [Frozen]Mock<IUpdateDetector> updateDetector,
            string newGroup,
            ConnectedClient sut)
        {
            var oldGroup = sut.Group;
            sut.Group = newGroup;

            updateDetector.Verify(x => x.MarkForUpdate(oldGroup));
            updateDetector.Verify(x => x.MarkForUpdate(newGroup));
        }

        [Theory, AutoMoqData]
        public void ShouldNotMarkCurrentGroupForUpdate(
            [Frozen]Mock<IUpdateDetector> updateDetector,
            string newGroup,
            ConnectedClient sut)
        {
            updateDetector.Verify(x => x.MarkForUpdate(sut.Group), Times.Never);

            sut.Group = newGroup;
            updateDetector.Verify(x => x.MarkForUpdate(newGroup), Times.Once);

            sut.Group = newGroup;
            updateDetector.Verify(x => x.MarkForUpdate(newGroup), Times.Once); // Still once.
        }
    }
}
