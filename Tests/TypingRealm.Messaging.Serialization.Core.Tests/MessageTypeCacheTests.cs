using System;
using System.Linq;
using Moq;
using TypingRealm.Messaging;
using TypingRealm.Testing;
using Xunit;

namespace Abc
{
    [Message]
    public class Test
    {
    }
}

namespace TypingRealm.Messaging.Serialization.A
{
    [Message]
    public class ATestMessageBeforeJsonSerializedMessage
    {
    }
}

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
        public void ShouldSortByTypeFullNameAndAssignFullNameAsTypeId()
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
            Assert.Equal(typeof(A.BTestMessage), all[typeof(A.BTestMessage).FullName!]);
            Assert.Equal(typeof(A.DTestMessage), all[typeof(A.DTestMessage).FullName!]);
            Assert.Equal(typeof(B.ATestMessage), all[typeof(B.ATestMessage).FullName!]);
            Assert.Equal(typeof(B.CTestMessage), all[typeof(B.CTestMessage).FullName!]);

            var list = all.ToList();
            Assert.Equal(typeof(A.BTestMessage), list[0].Value);
            Assert.Equal(typeof(A.DTestMessage), list[1].Value);
            Assert.Equal(typeof(B.ATestMessage), list[2].Value);
            Assert.Equal(typeof(B.CTestMessage), list[3].Value);
        }

        [Fact]
        public void ShouldPutMessageDataMessageAtFirstPlace()
        {
            var sut = new MessageTypeCache(new[]
            {
                typeof(A.DTestMessage),
                typeof(MessageData),
                typeof(Serialization.A.ATestMessageBeforeJsonSerializedMessage),
                typeof(Abc.Test)
            });

            var list = sut.GetAllTypes().ToDictionary(x => x.Key, x => x.Value)
                .ToList();

            Assert.Equal(typeof(MessageData), list[0].Value);
            Assert.Equal(typeof(Abc.Test), list[1].Value);
            Assert.Equal(typeof(Serialization.A.ATestMessageBeforeJsonSerializedMessage), list[2].Value);
            Assert.Equal(typeof(A.DTestMessage), list[3].Value);
        }

        [Fact]
        public void ShouldGetTypeById()
        {
            var sut = new MessageTypeCache(new[] { typeof(A.BTestMessage) });
            Assert.Equal(typeof(A.BTestMessage), sut.GetTypeById(typeof(A.BTestMessage).FullName!));
        }

        [Fact]
        public void ShouldGetIdByType()
        {
            var sut = new MessageTypeCache(new[] { typeof(A.BTestMessage) });
            Assert.Equal(typeof(A.BTestMessage).FullName!, sut.GetTypeId(typeof(A.BTestMessage)));
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

        [Theory, AutoMoqData]
        public void ShouldThrow_WhenTypeDoesNotHaveFullName(Mock<Type> type)
        {
            type.Setup(x => x.FullName).Returns((string?)null);

            Assert.Throws<InvalidOperationException>(
                () => new MessageTypeCache(new[] { typeof(A.BTestMessage), type.Object }));
        }
    }
}
