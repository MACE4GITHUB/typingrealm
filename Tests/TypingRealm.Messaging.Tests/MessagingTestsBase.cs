using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Moq;
using TypingRealm.Messaging.Updating;
using TypingRealm.Testing;

namespace TypingRealm.Messaging.Tests;

public abstract class MessagingTestsBase : TestsBase
{
    // TODO: Move these two last methods to Messaging assembly of tests.
    protected static void VerifyMarkedForUpdate(Mock<IUpdateDetector> updateDetector, string group, Times times = default)
    {
        updateDetector.Verify(x => x.MarkForUpdate(It.Is<IEnumerable<string>>(
            x => x.Count() == 1 && x.First() == group)), times);
    }

    protected ConnectedClient CreateMultiGroupClient(
        string[]? groups = null,
        IConnection? connection = null)
    {
        return new ConnectedClient(
            Fixture.Create<string>(),
            connection ?? Fixture.Create<IConnection>(),
            Fixture.Create<IUpdateDetector>(),
            groups ?? Fixture.Create<IEnumerable<string>>());
    }

    protected ConnectedClient CreateSingleGroupClient(
        IConnection? connection = null,
        IUpdateDetector? updateDetector = null)
    {
        return new ConnectedClient(
            Fixture.Create<string>(),
            connection ?? Fixture.Create<IConnection>(),
            updateDetector ?? Fixture.Create<IUpdateDetector>(),
            Fixture.Create<string>());
    }
}
