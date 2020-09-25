using System;
using System.Linq;
using Xunit;

namespace TypingRealm.Messaging.Serialization.Tests
{
    namespace A
    {
        [Message]
        public class BTestMessage
        {
        }

        [Message]
        public class DTestMessage
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
        public class CTestMessage
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
                    typeof(A.BTestMessage),
                    typeof(A.BTestMessage)
                }));
        }

        [Fact]
        public void ShouldBeEmptyWhenNoMessages()
        {
            var sut = new MessageTypeCache(Enumerable.Empty<Type>());
            Assert.Empty(sut.GetAllTypes());
        }

        [Fact]
        public void ShouldSortByTypeFullNameAndAssignShortNameAsTypeid()
        {
            var sut = new MessageTypeCache(new[]
            {
                typeof(A.DTestMessage),
                typeof(B.CTestMessage),
                typeof(B.ATestMessage),
                typeof(A.BTestMessage)
            });

            var all = sut.GetAllTypes().ToDictionary(x => x.Key, x => x.Value);
            Assert.Equal(4, all.Count);
            Assert.Equal(typeof(A.BTestMessage), all[nameof(A.BTestMessage)]);
            Assert.Equal(typeof(A.DTestMessage), all[nameof(A.DTestMessage)]);
            Assert.Equal(typeof(B.ATestMessage), all[nameof(B.ATestMessage)]);
            Assert.Equal(typeof(B.CTestMessage), all[nameof(B.CTestMessage)]);
        }

        [Fact]
        public void ShouldGetTypeById()
        {
            var sut = new MessageTypeCache(new[] { typeof(A.BTestMessage) });
            Assert.Equal(typeof(A.BTestMessage), sut.GetTypeById(nameof(A.BTestMessage)));
        }

        [Fact]
        public void ShouldGetIdByType()
        {
            var sut = new MessageTypeCache(new[] { typeof(A.BTestMessage) });
            Assert.Equal(nameof(A.BTestMessage), sut.GetTypeId(typeof(A.BTestMessage)));
        }

        [Fact]
        public void GetTypeById_ShouldThrowWhenTypeIsNotInCache()
        {
            var sut = new MessageTypeCache(new[] { typeof(A.BTestMessage) });
            Assert.Throws<InvalidOperationException>(
                () => sut.GetTypeById(nameof(B.CTestMessage)));
        }

        [Fact]
        public void GetTypeId_ShouldThrowWhenTypeIsNotInCache()
        {
            var sut = new MessageTypeCache(new[] { typeof(A.BTestMessage) });
            Assert.Throws<InvalidOperationException>(
                () => sut.GetTypeId(typeof(B.CTestMessage)));
        }
    }
}
