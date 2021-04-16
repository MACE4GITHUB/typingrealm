﻿using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Messaging.Tests
{
    public class ServerToClientMessageMetadataTests : TestsBase
    {
        [Fact]
        public void ShouldCreateEmpty()
        {
            var sut = ServerToClientMessageMetadata.CreateEmpty();

            Assert.Null(sut.RequestMessageId);
        }

        [Fact]
        public void ShouldBeSerializable()
        {
            AssertSerializable<ClientToServerMessageMetadata>();
        }
    }
}
