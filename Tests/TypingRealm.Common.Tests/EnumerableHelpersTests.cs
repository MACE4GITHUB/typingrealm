using System.Linq;
using Xunit;

namespace TypingRealm.Tests;

public class EnumerableHelpersTests
{
    [Fact]
    public void ShouldGenerateEnumerable()
    {
        var item = "item";

        var enumerable1 = EnumerableHelpers.AsEnumerable(item);
        var enumerable2 = EnumerableHelpers.AsEnumerable(item);

        Assert.False(ReferenceEquals(enumerable1, enumerable2));
        Assert.Equal(enumerable1, enumerable2);
        Assert.Single(enumerable1);
        Assert.Equal(item, enumerable1.Single());
    }
}
