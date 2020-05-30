using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Updating
{
    public class UpdaterTests : TestsBase
    {
        [Theory, AutoMoqData]
        public async Task ShouldSendUpdate(
            [Frozen]Mock<IUpdateFactory> updateFactory,
            ConnectedClient client,
            object update,
            Updater sut)
        {
            updateFactory.Setup(x => x.GetUpdateFor(client.ClientId))
                .Returns(update);

            await sut.SendUpdateAsync(client, Cts.Token);

            Mock.Get(client.Connection).Verify(x => x.SendAsync(update, Cts.Token));
        }
    }
}
