﻿using System.IO;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using TypingRealm.Messaging;

namespace TypingRealm.Testing
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(CreateFixture) { }

        public static Fixture CreateFixture()
        {
            var fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization());

            fixture.Register<Stream>(() => new MemoryStream());
            fixture.Customize<ConnectedClient>(x => x.OmitAutoProperties());

            return fixture;
        }
    }
}
