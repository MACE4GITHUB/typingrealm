using System.Collections.Generic;
using System.Linq;
using Moq;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests.Updating;

public class UpdateDetectorExtensionsTests
{
    [Theory, AutoMoqData]
    public void ShouldMarkSingleGroupForUpdate(
        string group,
        Mock<IUpdateDetector> sut)
    {
        sut.Object.MarkForUpdate(group);

        sut.Verify(x => x.MarkForUpdate(It.Is<IEnumerable<string>>(
            y => y.Count() == 1 && y.First() == group)));
    }
}
