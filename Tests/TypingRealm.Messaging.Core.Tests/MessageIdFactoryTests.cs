using System.Linq;
using System.Threading.Tasks;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests
{
    public class MessageIdFactoryTests
    {
        [Theory, AutoMoqData]
        public async Task ShouldMakeUniqueIdsConcurrently(MessageIdFactory sut)
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
        }
    }
}
