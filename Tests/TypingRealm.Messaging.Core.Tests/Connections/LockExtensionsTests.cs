using System.Threading.Tasks;
using Moq;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Connections;

public class LockExtensionsTests : TestsBase
{
    [Theory, AutoMoqData]
    public async Task UseWaitAsync_ShouldWaitForLock(ILock sut)
    {
        await sut.UseWaitAsync(Cts.Token);

        Mock.Get(sut).Verify(x => x.WaitAsync(Cts.Token));
    }

    [Theory, AutoMoqData]
    public async Task UseWaitAsync_ShouldReturnDisposableThatReleasesOnDispose(
        Mock<ILock> sut)
    {
        var result = await sut.Object.UseWaitAsync(Cts.Token);

        sut.Verify(x => x.ReleaseAsync(Cts.Token), Times.Never);

        await result.DisposeAsync();
        sut.Verify(x => x.ReleaseAsync(Cts.Token), Times.Once);

        // Should not call dispose twice.
        await result.DisposeAsync();
        sut.Verify(x => x.ReleaseAsync(Cts.Token), Times.Once);
    }
}
