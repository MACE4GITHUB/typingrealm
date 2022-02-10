using System;
using System.Linq;
using System.Threading.Tasks;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests;

public class MessageIdFactoryTests
{
    [Theory, AutoMoqData]
    public async Task ShouldMakeUniqueIdsConcurrently_InSequence(MessageIdFactory sut)
    {
        var results = Enumerable.Range(1, 1000)
            .Select(x => Task.Run(async () =>
            {
                await Task.Delay(10);
                return sut.CreateMessageId();
            }))
            .ToList();

        await Task.WhenAll(results);

        var count = results.Select(x => x.Result).Distinct().Count();
        Assert.Equal(1000, count);
        Assert.Equal(1, results.Select(x => Convert.ToInt32(x.Result)).Min());
        Assert.Equal(1000, results.Select(x => Convert.ToInt32(x.Result)).Max());
    }

    [Theory, AutoMoqData]
    public async Task ShouldHave10000MaxValue(MessageIdFactory sut)
    {
        var results = Enumerable.Range(1, 15000)
            .Select(x => Task.Run(async () =>
            {
                await Task.Delay(10);
                return sut.CreateMessageId();
            }))
            .ToList();

        await Task.WhenAll(results);

        var count = results.Select(x => x.Result).Distinct().Count();
        Assert.True(count >= 10000);
        Assert.True(count < 10100);

        Assert.Equal(1, results.Select(x => Convert.ToInt32(x.Result)).Min());
        Assert.True(results.Select(x => Convert.ToInt32(x.Result)).Max() < 10100);
        Assert.True(results.Select(x => Convert.ToInt32(x.Result)).Max() >= 10000);
        Assert.Equal(2, results.Select(x => x.Result).Count(x => x == "1"));
        Assert.Equal(2, results.Select(x => x.Result).Count(x => x == "4900"));
        Assert.Equal(1, results.Select(x => x.Result).Count(x => x == "5100"));
        Assert.Equal(1, results.Select(x => x.Result).Count(x => x == "10000"));
    }

    [Theory]
    [InlineAutoMoqData(2, 25000)]
    [InlineAutoMoqData(3, 35000)]
    [InlineAutoMoqData(6, 65000)]
    public async Task ShouldResetCounterMultipleTimes(
        int times, int totalCount, MessageIdFactory sut)
    {
        var results = Enumerable.Range(1, totalCount)
            .Select(x => Task.Run(async () =>
            {
                await Task.Delay(10);
                return sut.CreateMessageId();
            }))
            .ToList();

        await Task.WhenAll(results);

        var count = results.Select(x => x.Result).Distinct().Count();
        Assert.True(count >= 10000);
        Assert.True(count < 10100);

        Assert.Equal(1, results.Select(x => Convert.ToInt32(x.Result)).Min());
        Assert.True(results.Select(x => Convert.ToInt32(x.Result)).Max() < 10100);
        Assert.True(results.Select(x => Convert.ToInt32(x.Result)).Max() >= 10000);
        Assert.Equal(times + 1, results.Select(x => x.Result).Count(x => x == "1"));
        Assert.Equal(times + 1, results.Select(x => x.Result).Count(x => x == "4900"));
        Assert.Equal(times, results.Select(x => x.Result).Count(x => x == "5100"));
        Assert.Equal(times, results.Select(x => x.Result).Count(x => x == "10000"));
    }
}
