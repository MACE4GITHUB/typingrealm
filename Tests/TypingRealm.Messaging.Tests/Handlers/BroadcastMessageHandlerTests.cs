using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Handlers;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Tests.SpecimenBuilders;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Handlers
{
    public class BroadcastMessageHandlerTests : TestsBase
    {
        [Theory, SingleGroupData]
        public async Task ShouldResendMessageToEveryoneInTheSameGroupExceptTheSender(
            ConnectedClient sender,
            BroadcastMessage message,
            List<ConnectedClient> clientsInGroup,
            [Frozen]Mock<IConnectedClientStore> clients,
            BroadcastMessageHandler sut)
        {
            var senderId = sender.ClientId;
            Assert.NotEqual(senderId, message.SenderId);

            clients.Setup(x => x.FindInGroups(It.Is<IEnumerable<string>>(
                x => x.Count() == 1 && x.First() == sender.Group)))
                .Returns(clientsInGroup.Append(sender));

            await sut.HandleAsync(sender, message, Cts.Token);

            // Sync senderId to broadcasted message since it's senderId might be not set by front end.
            Assert.Equal(senderId, message.SenderId);

            Mock.Get(sender.Connection).Verify(x => x.SendAsync(It.IsAny<object>(), Cts.Token), Times.Never);

            foreach (var client in clientsInGroup)
            {
                Mock.Get(client.Connection).Verify(x => x.SendAsync(message, Cts.Token));
            }
        }
    }
}
