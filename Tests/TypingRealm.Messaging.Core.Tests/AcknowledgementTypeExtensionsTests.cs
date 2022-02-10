using Xunit;

namespace TypingRealm.Messaging.Tests;

public class AcknowledgementTypeExtensionsTests
{
    [Fact]
    public void ShouldNotRequireAcknowledgement_WhenAcknowledgementTypeIsNone()
    {
        var sut = AcknowledgementType.None;

        Assert.False(sut.IsAcknowledgementRequired());
    }

    [Theory]
    [InlineData(AcknowledgementType.Unspecified)]
    [InlineData(AcknowledgementType.Received)]
    [InlineData(AcknowledgementType.Handled)]
    [InlineData((AcknowledgementType)99)]
    public void ShouldRequireAcknowledgement_WhenAcknowledgementTypeIsNotNone(
        AcknowledgementType sut)
    {
        Assert.True(sut.IsAcknowledgementRequired());
    }
}
