using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Messages;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Updating
{
    public class AnnouncingUpdaterTests : TestsBase
    {
        [Theory, AutoMoqData]
        public async Task ShouldSendAnnounceMessage(
            [Frozen]Mock<IConnection> connection,
            ConnectedClient client,
            AnnouncingUpdater sut)
        {
            using var cts = new CancellationTokenSource();
            await sut.SendUpdateAsync(client, cts.Token);

            connection.Verify(x => x.SendAsync(It.IsAny<Announce>(), cts.Token));
        }
    }
}
