using System;
using System.Linq;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests
{
    namespace A
    {
        [Message]
        public class ATestMessage
        {
        }

        [Message]
        public class BTestMessage
        {
        }
    }

    namespace B
    {
        [Message]
        public class ATestMessage
        {
        }

        [Message]
        public class BTestMessage
        {
        }
    }

    public class MessageTypeCacheTests
    {
        [Fact]
        public void ShouldThrowIfSuppliedTypesContainDuplicates()
        {
            Assert.Throws<ArgumentException>(
                () => new MessageTypeCache(new[]
                {
                    typeof(A.ATestMessage),
                    typeof(A.ATestMessage)
                }));
        }

        [Fact]
        public void ShouldBeEmptyWhenNoMessages()
        {
            var sut = new MessageTypeCache(Enumerable.Empty<Type>());
            Assert.Empty(sut.GetAllTypes());
        }

        [Fact]
        public void ShouldSortByTypeFullNameAndAssignNumberStartingFrom1()
        {
            var sut = new MessageTypeCache(new[]
            {
                typeof(A.BTestMessage),
                typeof(B.BTestMessage),
                typeof(B.ATestMessage),
                typeof(A.ATestMessage)
            });

            var all = sut.GetAllTypes().ToDictionary(x => x.Key, x => x.Value);
            Assert.Equal(4, all.Count);
            Assert.Equal(typeof(A.ATestMessage), all["1"]);
            Assert.Equal(typeof(A.BTestMessage), all["2"]);
            Assert.Equal(typeof(B.ATestMessage), all["3"]);
            Assert.Equal(typeof(B.BTestMessage), all["4"]);
        }

        [Fact]
        public void ShouldGetTypeById()
        {
            var sut = new MessageTypeCache(new[] { typeof(A.ATestMessage) });
            Assert.Equal(typeof(A.ATestMessage), sut.GetTypeById("1"));
        }

        [Fact]
        public void ShouldGetIdByType()
        {
            var sut = new MessageTypeCache(new[] { typeof(A.ATestMessage) });
            Assert.Equal("1", sut.GetTypeId(typeof(A.ATestMessage)));
        }

        [Fact]
        public void GetTypeById_ShouldThrowWhenTypeIsNotInCache()
        {
            var sut = new MessageTypeCache(new[] { typeof(A.ATestMessage) });
            Assert.Throws<InvalidOperationException>(
                () => sut.GetTypeById("2"));
        }

        [Fact]
        public void GetTypeId_ShouldThrowWhenTypeIsNotInCache()
        {
            var sut = new MessageTypeCache(new[] { typeof(A.ATestMessage) });
            Assert.Throws<InvalidOperationException>(
                () => sut.GetTypeId(typeof(B.BTestMessage)));
        }
    }
}
